using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Hosting;
using Koncilia_Contratos.Data;
using Microsoft.EntityFrameworkCore;

namespace Koncilia_Contratos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IServiceProvider _serviceProvider;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _serviceProvider = serviceProvider;
        }

        public async Task SendBirthdayEmailAsync(string toEmail, string nombre, string apellido, List<string>? bccEmails = null)
        {
            var nombreCompleto = $"{nombre} {apellido}";
            var subject = $"¡Feliz Cumpleaños {nombre}! 🎉";
            
            // Seleccionar una imagen aleatoria de los disponibles (.gif, .png, .jpg, .jpeg)
            string? imageFileName = null;
            try
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday");
                if (Directory.Exists(imagePath))
                {
                    var imageFiles = Directory.GetFiles(imagePath)
                        .Where(f => {
                            var ext = Path.GetExtension(f).ToLower();
                            return ext == ".gif" || ext == ".png" || ext == ".jpg" || ext == ".jpeg";
                        })
                        .ToArray();
                    
                    if (imageFiles.Length > 0)
                    {
                        // Seleccionar una imagen aleatoria
                        var random = new Random();
                        var randomIndex = random.Next(0, imageFiles.Length);
                        imageFileName = Path.GetFileName(imageFiles[randomIndex]);
                        _logger.LogInformation($"Imagen aleatoria seleccionada: {imageFileName} (de {imageFiles.Length} disponibles)");
                    }
                    else
                    {
                        _logger.LogWarning("No se encontraron archivos de imagen en la carpeta de cumpleaños");
                    }
                }
                else
                {
                    _logger.LogWarning("La carpeta de imágenes de cumpleaños no existe");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al seleccionar imagen aleatoria: {Message}", ex.Message);
            }
            
            // Si no hay imagen, no enviar correo
            if (string.IsNullOrEmpty(imageFileName))
            {
                _logger.LogWarning($"No se puede enviar correo a {toEmail}: No hay imagen disponible");
                return;
            }
            
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            margin: 0;
            padding: 20px;
            background: #ffffff;
        }}
        .gif-container {{
            text-align: center;
            margin: 0;
            padding: 0;
        }}
        .gif-container img {{
            max-width: 100%;
            height: auto;
            display: block;
            margin: 0 auto;
        }}
    </style>
</head>
<body>
    <div class='gif-container'>
        <img src='cid:birthday-image' alt='Feliz Cumpleaños' />
    </div>
</body>
</html>";

            await SendEmailWithAttachmentAsync(toEmail, subject, body, imageFileName, bccEmails);
        }

        private async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string? imageFileName, List<string>? bccEmails = null)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["Email:FromName"] ?? "Koncilia Contratos";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Configuración de email no encontrada. No se puede enviar el correo.");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                
                // Agregar BCC a todos los demás empleados si se proporcionan
                if (bccEmails != null && bccEmails.Any())
                {
                    foreach (var bccEmail in bccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(bccEmail) && bccEmail != toEmail)
                        {
                            message.Bcc.Add(new MailboxAddress("", bccEmail));
                        }
                    }
                    _logger.LogInformation($"Se agregaron {message.Bcc.Count} correos en BCC");
                }
                
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;

                // Agregar la imagen como attachment inline si existe
                if (!string.IsNullOrEmpty(imageFileName))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday", imageFileName);
                    if (File.Exists(imagePath))
                    {
                        var attachment = bodyBuilder.LinkedResources.Add(imagePath);
                        attachment.ContentId = "birthday-image";
                        attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                        attachment.ContentDisposition.FileName = imageFileName;
                        _logger.LogInformation($"Imagen agregada al correo: {imageFileName}");
                    }
                    else
                    {
                        _logger.LogWarning($"Imagen no encontrada en la ruta: {imagePath}");
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Correo enviado exitosamente a {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo a {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["Email:FromName"] ?? "Koncilia Contratos";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Configuración de email no encontrada. No se puede enviar el correo.");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Correo enviado exitosamente a {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo a {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}

