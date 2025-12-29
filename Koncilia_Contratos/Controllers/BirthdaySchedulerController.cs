using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;
using Koncilia_Contratos.Services;

namespace Koncilia_Contratos.Controllers
{
    /// <summary>
    /// Controlador para ejecutar la verificación de cumpleaños mediante llamadas HTTP externas
    /// Útil para servicios de cron jobs cuando Always On no está disponible en Azure App Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BirthdaySchedulerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<BirthdaySchedulerController> _logger;
        private readonly IConfiguration _configuration;
        private const string DefaultTimeZoneId = "SA Pacific Standard Time"; // Bogotá/Lima/Quito (UTC-5 sin DST)

        public BirthdaySchedulerController(
            ApplicationDbContext context, 
            IEmailService emailService, 
            ILogger<BirthdaySchedulerController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Endpoint para ejecutar la verificación de cumpleaños manualmente
        /// Requiere una clave de seguridad configurada en appsettings.json
        /// </summary>
        /// <param name="key">Clave de seguridad para autenticar la solicitud</param>
        /// <returns>Resultado de la verificación</returns>
        [HttpGet("check")]
        [HttpPost("check")]
        public async Task<IActionResult> CheckBirthdays([FromQuery] string? key)
        {
            // Validar clave de seguridad
            var expectedKey = _configuration["Birthday:SchedulerKey"];
            if (string.IsNullOrEmpty(expectedKey))
            {
                _logger.LogWarning("Birthday:SchedulerKey no está configurado en appsettings.json. El endpoint está deshabilitado por seguridad.");
                return Unauthorized(new { error = "Endpoint no configurado correctamente" });
            }

            if (key != expectedKey)
            {
                _logger.LogWarning("Intento de acceso al endpoint de verificación de cumpleaños con clave incorrecta desde IP: {IpAddress}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return Unauthorized(new { error = "Clave inválida" });
            }

            try
            {
                _logger.LogInformation("Verificación de cumpleaños iniciada mediante endpoint HTTP");

                // Obtener zona horaria configurada
                var timeZoneId = _configuration["Birthday:TimeZoneId"] ?? DefaultTimeZoneId;
                TimeZoneInfo timeZone;

                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
                }

                // Usar la fecha de la zona horaria configurada
                var hoy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;

                // Obtener empleados que cumplen años hoy
                var empleadosCumpleanos = await _context.Empleados
                    .Where(e => e.FechaCumpleanos.Month == hoy.Month &&
                               e.FechaCumpleanos.Day == hoy.Day)
                    .ToListAsync();

                if (!empleadosCumpleanos.Any())
                {
                    _logger.LogInformation("No hay empleados que cumplan años hoy ({Fecha})", hoy);
                    return Ok(new 
                    { 
                        success = true, 
                        message = "No hay empleados que cumplan años hoy",
                        fecha = hoy.ToString("yyyy-MM-dd"),
                        empleadosEncontrados = 0
                    });
                }

                _logger.LogInformation("Se encontraron {Cantidad} empleado(s) que cumple(n) años hoy.", empleadosCumpleanos.Count);

                // Obtener todos los correos de empleados para enviar copia (BCC)
                var todosLosEmpleados = await _context.Empleados
                    .Where(e => !string.IsNullOrEmpty(e.CorreoElectronico))
                    .Select(e => e.CorreoElectronico)
                    .ToListAsync();

                var resultados = new List<object>();
                int enviadosExitosos = 0;
                int errores = 0;

                foreach (var empleado in empleadosCumpleanos)
                {
                    try
                    {
                        // Crear lista de BCC excluyendo al empleado que cumple años
                        var bccEmails = todosLosEmpleados
                            .Where(e => e != empleado.CorreoElectronico)
                            .ToList();

                        await _emailService.SendBirthdayEmailAsync(
                            empleado.CorreoElectronico,
                            empleado.Nombre,
                            empleado.Apellido,
                            bccEmails);

                        _logger.LogInformation("Correo de cumpleaños enviado exitosamente a {NombreCompleto} ({Email}) con copia a {CantidadBcc} empleado(s)",
                            empleado.NombreCompleto, empleado.CorreoElectronico, bccEmails.Count);

                        resultados.Add(new
                        {
                            empleado = empleado.NombreCompleto,
                            email = empleado.CorreoElectronico,
                            estado = "enviado",
                            bccCount = bccEmails.Count
                        });

                        enviadosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar correo de cumpleaños a {NombreCompleto}: {Error}",
                            empleado.NombreCompleto, ex.Message);

                        resultados.Add(new
                        {
                            empleado = empleado.NombreCompleto,
                            email = empleado.CorreoElectronico,
                            estado = "error",
                            error = ex.Message
                        });

                        errores++;
                    }
                }

                _logger.LogInformation("Verificación de cumpleaños completada. Enviados: {Enviados}, Errores: {Errores}", 
                    enviadosExitosos, errores);

                return Ok(new
                {
                    success = true,
                    message = $"Verificación completada. Enviados: {enviadosExitosos}, Errores: {errores}",
                    fecha = hoy.ToString("yyyy-MM-dd"),
                    empleadosEncontrados = empleadosCumpleanos.Count,
                    enviadosExitosos,
                    errores,
                    resultados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al verificar cumpleaños: {Error}", ex.Message);
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = "Error al procesar la verificación de cumpleaños",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint simple de keep-alive para mantener la aplicación activa
        /// No requiere autenticación, pero es útil para servicios de monitoreo
        /// </summary>
        [HttpGet("ping")]
        [HttpPost("ping")]
        public IActionResult Ping()
        {
            return Ok(new 
            { 
                status = "ok", 
                timestamp = DateTime.UtcNow,
                message = "Servicio activo"
            });
        }
    }
}



