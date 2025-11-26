namespace Koncilia_Contratos.Services
{
    public interface IEmailService
    {
        Task SendBirthdayEmailAsync(string toEmail, string nombre, string apellido);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}

