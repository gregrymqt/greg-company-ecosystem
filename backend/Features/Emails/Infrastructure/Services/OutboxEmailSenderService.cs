using System.Text.Json;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeuCrudCsharp.Features.Emails.Infrastructure.Services;

public class OutboxEmailSenderService : IEmailSenderService
{
    private readonly IMongoDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxEmailSenderService> _logger;

    public OutboxEmailSenderService(
        IMongoDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<OutboxEmailSenderService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string plainTextBody)
    {
        var outboxEvent = new OutboxEvent
        {
            EventType = "email.send.requested",
            Payload = JsonSerializer.Serialize(new
            {
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody
            })
        };

        var outboxCollection = _context.GetCollection<OutboxEvent>("OutboxEvents");

        if (_unitOfWork.Session != null)
        {
            await outboxCollection.InsertOneAsync(_unitOfWork.Session, outboxEvent);
        }
        else
        {
            await outboxCollection.InsertOneAsync(outboxEvent);
        }

        _logger.LogInformation("Evento de envio de e-mail para {To} gravado no Outbox (Id: {EventId}).", to, outboxEvent.Id);
    }
}
