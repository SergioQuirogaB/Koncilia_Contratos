using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Koncilia_Contratos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBirthdayEmailAsync(string toEmail, string nombre, string apellido)
        {
            var nombreCompleto = $"{nombre} {apellido}";
            var subject = $"¡Feliz Cumpleaños {nombre}! 🎉";
            
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

            await SendEmailAsync(toEmail, subject, body, true);
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

