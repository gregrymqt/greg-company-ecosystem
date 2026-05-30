using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Emails.Application.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, string plainTextBody);
    }
}
