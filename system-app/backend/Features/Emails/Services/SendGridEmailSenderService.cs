using MeuCrudCsharp.Features.Emails.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MeuCrudCsharp.Features.Emails.Services
{
    public class SendGridEmailSenderService : IEmailSenderService
    {
        private readonly SendGridSettings _settings;
        private readonly ILogger<SendGridEmailSenderService> _logger;

        public SendGridEmailSenderService(
            IOptions<SendGridSettings> options,
            ILogger<SendGridEmailSenderService> logger
        )
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string htmlBody,
            string plainTextBody
        )
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("O endereço de e-mail do destinatário não pode ser vazio.", nameof(to));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("O assunto do e-mail não pode ser vazio.", nameof(subject));

            if (string.IsNullOrWhiteSpace(htmlBody) && string.IsNullOrWhiteSpace(plainTextBody))
                throw new ArgumentException("O e-mail deve conter pelo menos um corpo (HTML ou texto simples).");

            try
            {
                var apiKey = _settings.ApiKey;
                var fromEmail = _settings.FromEmail;
                var fromName = _settings.FromName;

                if (
                    string.IsNullOrEmpty(apiKey)
                    || string.IsNullOrEmpty(fromEmail)
                    || string.IsNullOrEmpty(fromName)
                )
                {
                    throw new InvalidOperationException(
                        "As configurações do SendGrid (ApiKey, FromEmail, FromName) não foram definidas corretamente."
                    );
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(
                    from,
                    toAddress,
                    subject,
                    plainTextBody,
                    htmlBody
                );

                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("E-mail para {To} enviado com sucesso.", to);
                }
                else
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError(
                        "Falha ao enviar e-mail para {To}. Status: {StatusCode}. Resposta: {ResponseBody}",
                        to,
                        response.StatusCode,
                        responseBody
                    );
                    throw new AppServiceException(
                        $"Falha ao enviar e-mail. Status: {response.StatusCode}. Detalhes: {responseBody}"
                    );
                }
            }
            catch (AppServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao enviar e-mail para {To}.", to);
                throw new AppServiceException("Ocorreu um erro ao tentar enviar o e-mail.", ex);
            }
        }
    }
}
