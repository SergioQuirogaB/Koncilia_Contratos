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

        // Hora configurada para enviar correos (por defecto 8:00 AM)
        private readonly TimeSpan _scheduledTime = new TimeSpan(9, 0, 0); // 8:00 AM

        public BirthdayBackgroundService(IServiceProvider serviceProvider, ILogger<BirthdayBackgroundService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de verificación de cumpleaños iniciado.");
            
            // Leer la hora configurada desde appsettings.json si existe
            var horaConfigurada = _configuration["Birthday:CheckTime"]; // Formato: "08:00"
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
                await CheckBirthdaysAsync();
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
                    // Calcular el tiempo hasta la próxima ejecución programada
                    var ahora = DateTime.Now;
                    var proximaEjecucion = ahora.Date.Add(scheduledTime);
                    
                    // Si la hora programada ya pasó hoy, programar para mañana
                    if (proximaEjecucion <= ahora)
                    {
                        proximaEjecucion = proximaEjecucion.AddDays(1);
                    }

                    var tiempoRestante = proximaEjecucion - ahora;
                    _logger.LogInformation($"Próxima verificación de cumpleaños programada para: {proximaEjecucion:yyyy-MM-dd HH:mm:ss} (en {tiempoRestante.TotalHours:F2} horas)");

                    // Esperar hasta la hora programada
                    await Task.Delay(tiempoRestante, stoppingToken);

                    // Ejecutar verificación
                    await CheckBirthdaysAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar cumpleaños: {Error}", ex.Message);
                    
                    // Si hay un error, esperar 1 hora antes de reintentar
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CheckBirthdaysAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var hoy = DateTime.Today;
                
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

