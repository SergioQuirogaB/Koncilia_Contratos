using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Koncilia_Contratos.Services
{
    public class BirthdayBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BirthdayBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        // Hora configurada para enviar correos (por defecto 9:00 AM hora Bogotá)
        private readonly TimeSpan _scheduledTime = new TimeSpan(9, 0, 0);
        private const string DefaultTimeZoneId = "SA Pacific Standard Time"; // Bogotá/Lima/Quito (UTC-5 sin DST)

        public BirthdayBackgroundService(IServiceProvider serviceProvider, ILogger<BirthdayBackgroundService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de verificación de cumpleaños iniciado.");
            
            // Zona horaria: configurable y con valor por defecto en Bogotá
            var timeZoneId = _configuration["Birthday:TimeZoneId"] ?? DefaultTimeZoneId;
            TimeZoneInfo? timeZone;

            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                _logger.LogInformation("Zona horaria para cumpleaños: {TimeZoneId} (UTC{Offset})",
                    timeZone.Id, timeZone.BaseUtcOffset);
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("Zona horaria {TimeZoneId} no encontrada. Usando {DefaultTimeZoneId}.", timeZoneId, DefaultTimeZoneId);
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
            }
            catch (InvalidTimeZoneException ex)
            {
                _logger.LogWarning(ex, "Zona horaria {TimeZoneId} inválida. Usando {DefaultTimeZoneId}.", timeZoneId, DefaultTimeZoneId);
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
            }

            // Leer la hora configurada desde appsettings.json si existe
            var horaConfigurada = _configuration["Birthday:CheckTime"]; // Formato: "09:00"
            var scheduledTime = _scheduledTime;
            
            if (!string.IsNullOrEmpty(horaConfigurada))
            {
                if (TimeSpan.TryParse(horaConfigurada, out var parsedTime))
                {
                    scheduledTime = parsedTime;
                    _logger.LogInformation($"Hora de verificación de cumpleaños configurada: {scheduledTime:hh\\:mm}");
                }
            }
            else
            {
                _logger.LogInformation($"Hora de verificación de cumpleaños: {scheduledTime:hh\\:mm} (predeterminado)");
            }

            // Ejecutar verificación inicial (con manejo de errores)
            try
            {
                await CheckBirthdaysAsync(timeZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la verificación inicial de cumpleaños: {Error}", ex.Message);
                // Continuar aunque haya error, para no detener el servicio
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calcular el tiempo hasta la próxima ejecución programada usando zona horaria configurada
                    var ahoraUtc = DateTime.UtcNow;
                    var ahoraZona = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, timeZone);

                    var proximaEjecucionZona = ahoraZona.Date.Add(scheduledTime);
                    
                    // Si la hora programada ya pasó hoy, programar para mañana
                    if (proximaEjecucionZona <= ahoraZona)
                    {
                        proximaEjecucionZona = proximaEjecucionZona.AddDays(1);
                    }

                    // Convertir la próxima ejecución a UTC para calcular el delay real
                    var proximaEjecucionUtc = TimeZoneInfo.ConvertTimeToUtc(proximaEjecucionZona, timeZone);
                    var tiempoRestante = proximaEjecucionUtc - ahoraUtc;

                    _logger.LogInformation(
                        "Próxima verificación de cumpleaños programada para: {FechaZona} (hora local configurada) | UTC: {FechaUtc} (en {Horas:F2} horas)",
                        proximaEjecucionZona, proximaEjecucionUtc, tiempoRestante.TotalHours);

                    // Esperar hasta la hora programada
                    await Task.Delay(tiempoRestante, stoppingToken);

                    // Ejecutar verificación
                    await CheckBirthdaysAsync(timeZone);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar cumpleaños: {Error}", ex.Message);
                    
                    // Si hay un error, esperar 1 hora antes de reintentar
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CheckBirthdaysAsync(TimeZoneInfo timeZone)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Usar la fecha de Bogotá (o la zona configurada) para evitar desfases con la región del App Service
                var hoy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
                
                // Obtener empleados que cumplen años hoy
                var empleadosCumpleanos = await dbContext.Empleados
                    .Where(e => e.FechaCumpleanos.Month == hoy.Month && 
                               e.FechaCumpleanos.Day == hoy.Day)
                    .ToListAsync();

                if (empleadosCumpleanos.Any())
                {
                    _logger.LogInformation($"Se encontraron {empleadosCumpleanos.Count} empleado(s) que cumple(n) años hoy.");

                    // Obtener todos los correos de empleados para enviar copia (BCC)
                    var todosLosEmpleados = await dbContext.Empleados
                        .Where(e => !string.IsNullOrEmpty(e.CorreoElectronico))
                        .Select(e => e.CorreoElectronico)
                        .ToListAsync();

                    foreach (var empleado in empleadosCumpleanos)
                    {
                        try
                        {
                            // Crear lista de BCC excluyendo al empleado que cumple años
                            var bccEmails = todosLosEmpleados
                                .Where(e => e != empleado.CorreoElectronico)
                                .ToList();

                            await emailService.SendBirthdayEmailAsync(
                                empleado.CorreoElectronico, 
                                empleado.Nombre, 
                                empleado.Apellido,
                                bccEmails);

                            _logger.LogInformation($"Correo de cumpleaños enviado exitosamente a {empleado.NombreCompleto} ({empleado.CorreoElectronico}) con copia a {bccEmails.Count} empleado(s)");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error al enviar correo de cumpleaños a {empleado.NombreCompleto}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No hay empleados que cumplan años hoy.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar cumpleaños en CheckBirthdaysAsync: {Error}", ex.Message);
                // No relanzar la excepción para evitar que detenga el servicio
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de verificación de cumpleaños detenido.");
            await base.StopAsync(cancellationToken);
        }
    }
}

