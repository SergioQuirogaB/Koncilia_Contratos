using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Hosting;

namespace Koncilia_Contratos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendBirthdayEmailAsync(string toEmail, string nombre, string apellido, List<string>? bccEmails = null)
        {
            var nombreCompleto = $"{nombre} {apellido}";
            var subject = $"¡Feliz Cumpleaños {nombre}! 🎉";
            
            // Buscar el GIF en la carpeta birthday
            var gifPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday");
            var gifFiles = Directory.GetFiles(gifPath, "*.gif");
            var gifFileName = gifFiles.Length > 0 ? Path.GetFileName(gifFiles[0]) : null;
            
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .message {{
            font-size: 18px;
            margin-bottom: 20px;
        }}
        .gif-container {{
            text-align: center;
            margin: 20px 0;
        }}
        .gif-container img {{
            max-width: 100%;
            height: auto;
            border-radius: 10px;
        }}
        .signature {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 2px solid #ddd;
            font-style: italic;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 ¡Feliz Cumpleaños! 🎂</h1>
    </div>
    <div class='content'>
        <p class='message'>
            <strong>¡Hola {nombre}!</strong>
        </p>
        <p>
            Queremos desearte un <strong>¡Feliz Cumpleaños!</strong> en este día tan especial. 
            Esperamos que este nuevo año de vida esté lleno de alegría, éxito y muchas 
            bendiciones.
        </p>
        {(gifFileName != null ? $@"
        <div class='gif-container'>
            <img src='cid:birthday-gif' alt='Feliz Cumpleaños' />
        </div>
        " : "")}
        <p>
            Que todos tus sueños se hagan realidad y que este día esté lleno de momentos 
            inolvidables junto a tus seres queridos.
        </p>
        <p>
            ¡Que disfrutes mucho tu día! 🎈🎊
        </p>
        <div class='signature'>
            <p>Con mucho cariño,</p>
            <p><strong>Equipo Koncilia</strong></p>
        </div>
    </div>
</body>
</html>";

            await SendEmailWithAttachmentAsync(toEmail, subject, body, gifFileName, bccEmails);
        }

        private async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string? gifFileName, List<string>? bccEmails = null)
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

                // Agregar el GIF como attachment inline si existe
                if (!string.IsNullOrEmpty(gifFileName))
                {
                    var gifPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday", gifFileName);
                    if (File.Exists(gifPath))
                    {
                        var attachment = bodyBuilder.LinkedResources.Add(gifPath);
                        attachment.ContentId = "birthday-gif";
                        attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                        attachment.ContentDisposition.FileName = gifFileName;
                        _logger.LogInformation($"GIF agregado al correo: {gifFileName}");
                    }
                    else
                    {
                        _logger.LogWarning($"GIF no encontrado en la ruta: {gifPath}");
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

