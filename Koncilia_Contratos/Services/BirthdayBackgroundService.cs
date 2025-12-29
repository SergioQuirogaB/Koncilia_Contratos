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
        // IMPORTANTE: Esta hora SIEMPRE será a las 9:00 AM hora de Bogotá, sin importar la temporada (horario de verano o estándar)
        private readonly TimeSpan _scheduledTime = new TimeSpan(9, 0, 0);
        // SA Pacific Standard Time = Bogotá/Lima/Quito (UTC-5 SIN horario de verano, siempre la misma hora)
        private const string DefaultTimeZoneId = "SA Pacific Standard Time";

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
                
                // Verificar que la zona horaria no tenga horario de verano (DST)
                // Esto asegura que siempre será la misma hora sin importar la temporada
                if (timeZone.SupportsDaylightSavingTime)
                {
                    _logger.LogWarning(
                        "⚠️ ADVERTENCIA: La zona horaria {TimeZoneId} SOPORTA horario de verano (DST). " +
                        "Esto puede causar que la hora de envío varíe según la temporada. " +
                        "Se recomienda usar 'SA Pacific Standard Time' que NO tiene DST.",
                        timeZone.Id);
                }
                else
                {
                    _logger.LogInformation(
                        "✅ Zona horaria {TimeZoneId} NO tiene horario de verano (DST). " +
                        "Los correos SIEMPRE se enviarán a la misma hora ({HoraProgramada}) sin importar la temporada.",
                        timeZone.Id, scheduledTime.ToString(@"hh\:mm"));
                }
                
                // Obtener información sobre la zona horaria del servidor
                var serverTimeZone = TimeZoneInfo.Local;
                var ahoraUtc = DateTime.UtcNow;
                var offsetConfigurado = timeZone.GetUtcOffset(ahoraUtc);
                var offsetServidor = serverTimeZone.GetUtcOffset(ahoraUtc);
                
                _logger.LogInformation(
                    "Zona horaria para cumpleaños: {TimeZoneId} (Offset: UTC{Offset}, Sin DST: {SinDST}) | " +
                    "Zona horaria del servidor: {ServerTimeZone} (Offset: UTC{ServerOffset})",
                    timeZone.Id, offsetConfigurado, !timeZone.SupportsDaylightSavingTime, 
                    serverTimeZone.Id, offsetServidor);
                
                // Advertencia si hay diferencia significativa entre zonas horarias
                if (Math.Abs((offsetConfigurado - offsetServidor).TotalHours) > 0.5)
                {
                    _logger.LogInformation(
                        "ℹ️ El servidor está en {ServerTimeZone} pero los correos se enviarán usando {ConfigTimeZone}. " +
                        "Los correos SIEMPRE se enviarán a las {HoraProgramada} hora de {ConfigTimeZone}, sin importar la hora del servidor.",
                        serverTimeZone.Id, timeZone.Id, scheduledTime.ToString(@"hh\:mm"), timeZone.Id);
                }
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
                    _logger.LogInformation(
                        "Hora de verificación de cumpleaños configurada: {Hora} (hora {TimeZoneId}). " +
                        "Los correos SIEMPRE se enviarán a esta hora, sin importar la temporada.",
                        scheduledTime.ToString(@"hh\:mm"), timeZone.Id);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Hora de verificación de cumpleaños: {Hora} (predeterminado, hora {TimeZoneId}). " +
                    "Los correos SIEMPRE se enviarán a esta hora, sin importar la temporada.",
                    scheduledTime.ToString(@"hh\:mm"), timeZone.Id);
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

                    // Obtener también la hora del servidor para comparación
                    var horaServidor = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, TimeZoneInfo.Local);
                    
                    _logger.LogInformation(
                        "Estado actual - UTC: {AhoraUtc} | Hora servidor (East US 2): {HoraServidor} | Hora local configurada ({TimeZoneId}): {AhoraZona} | Hora programada: {HoraProgramada}",
                        ahoraUtc, horaServidor, timeZone.Id, ahoraZona, scheduledTime);

                    // Calcular la próxima ejecución usando SOLO la fecha y la hora programada
                    // Esto asegura que SIEMPRE sea exactamente la hora configurada (ej: 9:00 AM), sin importar la temporada
                    var proximaEjecucionZona = ahoraZona.Date.Add(scheduledTime);
                    
                    // Si la hora programada ya pasó hoy, programar para mañana
                    if (proximaEjecucionZona <= ahoraZona)
                    {
                        proximaEjecucionZona = proximaEjecucionZona.AddDays(1);
                        _logger.LogInformation(
                            "La hora programada ({Hora}) ya pasó hoy. Programando para mañana: {ProximaEjecucion} (hora {TimeZoneId})",
                            scheduledTime.ToString(@"hh\:mm"), proximaEjecucionZona, timeZone.Id);
                    }
                    
                    // Validar que la hora programada sea exactamente la configurada (medida de seguridad)
                    var horaRealProgramada = proximaEjecucionZona.TimeOfDay;
                    if (Math.Abs((horaRealProgramada - scheduledTime).TotalMinutes) > 1)
                    {
                        _logger.LogWarning(
                            "⚠️ La hora programada ({HoraReal}) no coincide exactamente con la hora configurada ({HoraConfigurada}). " +
                            "Corrigiendo a la hora exacta...",
                            horaRealProgramada.ToString(@"hh\:mm"), scheduledTime.ToString(@"hh\:mm"));
                        proximaEjecucionZona = proximaEjecucionZona.Date.Add(scheduledTime);
                    }

                    // Convertir la próxima ejecución a UTC para calcular el delay real
                    // IMPORTANTE: Usamos la zona horaria configurada (sin DST) para asegurar que siempre sea la misma hora
                    // La zona horaria "SA Pacific Standard Time" NO tiene horario de verano, por lo que siempre será UTC-5
                    var proximaEjecucionUtc = TimeZoneInfo.ConvertTimeToUtc(proximaEjecucionZona, timeZone);
                    var tiempoRestante = proximaEjecucionUtc - ahoraUtc;

                    // Validar que el tiempo restante sea razonable (no negativo y no mayor a 25 horas)
                    if (tiempoRestante.TotalMilliseconds < 0)
                    {
                        _logger.LogWarning("Tiempo restante negativo detectado. Recalculando para mañana.");
                        proximaEjecucionZona = ahoraZona.Date.AddDays(1).Add(scheduledTime);
                        proximaEjecucionUtc = TimeZoneInfo.ConvertTimeToUtc(proximaEjecucionZona, timeZone);
                        tiempoRestante = proximaEjecucionUtc - ahoraUtc;
                    }

                    if (tiempoRestante.TotalHours > 25)
                    {
                        _logger.LogWarning("Tiempo restante mayor a 25 horas detectado. Esto puede indicar un problema con la zona horaria.");
                    }

                    // Calcular también la hora en la zona del servidor para referencia
                    var proximaEjecucionServidor = TimeZoneInfo.ConvertTimeFromUtc(proximaEjecucionUtc, TimeZoneInfo.Local);
                    
                    _logger.LogInformation(
                        "Próxima verificación de cumpleaños programada para: {FechaZona} (hora {TimeZoneId}) | " +
                        "Hora servidor (East US 2): {FechaServidor} | UTC: {FechaUtc} | " +
                        "Tiempo restante: {Horas:F2} horas ({Minutos:F0} minutos)",
                        proximaEjecucionZona, timeZone.Id, proximaEjecucionServidor, proximaEjecucionUtc, 
                        tiempoRestante.TotalHours, tiempoRestante.TotalMinutes);

                    // Esperar hasta la hora programada
                    await Task.Delay(tiempoRestante, stoppingToken);

                    // Verificar nuevamente la hora antes de ejecutar (por si hubo algún desfase)
                    var ahoraUtcDespues = DateTime.UtcNow;
                    var ahoraZonaDespues = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtcDespues, timeZone);
                    
                    var horaServidorDespues = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtcDespues, TimeZoneInfo.Local);
                    
                    _logger.LogInformation(
                        "Ejecutando verificación de cumpleaños. Hora actual - UTC: {AhoraUtc} | Hora servidor (East US 2): {HoraServidor} | Hora local configurada ({TimeZoneId}): {AhoraZona}",
                        ahoraUtcDespues, horaServidorDespues, timeZone.Id, ahoraZonaDespues);

                    // Ejecutar verificación
                    await CheckBirthdaysAsync(timeZone);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Servicio de cumpleaños cancelado.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar cumpleaños: {Error}", ex.Message);
                    
                    // Si hay un error, esperar 1 hora antes de reintentar
                    _logger.LogInformation("Reintentando en 1 hora...");
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
                var ahoraUtc = DateTime.UtcNow;
                var ahoraZona = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, timeZone);
                var hoy = ahoraZona.Date;
                
                var horaServidor = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, TimeZoneInfo.Local);
                
                _logger.LogInformation(
                    "Verificando cumpleaños para la fecha: {Fecha} | UTC: {Utc} | Hora servidor (East US 2): {HoraServidor} | Hora local configurada ({TimeZoneId}): {HoraLocal}",
                    hoy.ToString("yyyy-MM-dd"), ahoraUtc, horaServidor, timeZone.Id, ahoraZona);
                
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

