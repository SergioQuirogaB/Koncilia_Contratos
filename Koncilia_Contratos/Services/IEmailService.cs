namespace Koncilia_Contratos.Services
{
    public interface IEmailService
    {
        Task SendBirthdayEmailAsync(string toEmail, string nombre, string apellido, List<string>? bccEmails = null);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}

