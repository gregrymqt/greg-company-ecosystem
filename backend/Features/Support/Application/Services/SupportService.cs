using MeuCrudCsharp.Features.Support.Domain.Interfaces;
using MeuCrudCsharp.Features.Support.Domain.Entities;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Support.Application.DTOs;
using MeuCrudCsharp.Features.Support.Application.Interfaces;
using MeuCrudCsharp.Features.Support.Application.Utils;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using System.Text.Json;

namespace MeuCrudCsharp.Features.Support.Application.Services
{
    public class SupportService : ISupportService
    {
        private readonly ISupportRepository _repository;
        private readonly IMongoDbContext _mongoContext;

        public SupportService(ISupportRepository repository, IMongoDbContext mongoContext)
        {
            _repository = repository;
            _mongoContext = mongoContext;
        }

        public async Task CreateTicketAsync(string userId, CreateSupportTicketDto dto)
        {
            var document = new SupportTicket
            {
                UserId = userId,
                Title = dto.Title,
                Category = dto.Category,
                Priority = dto.Priority,
                Status = "open",
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Responses = new List<SupportResponse>
                {
                    new SupportResponse
                    {
                        SenderId = userId,
                        SenderRole = "Student",
                        Message = dto.Message,
                        DateCreated = DateTime.UtcNow
                    }
                }
            };

            await _repository.CreateAsync(document);
        }

        public async Task<SupportTicketResponseDto> GetTicketByIdAsync(string id)
        {
            var ticket = await _repository.GetByIdAsync(id);

            if (ticket == null)
            {
                throw new ResourceNotFoundException("Ticket de suporte não encontrado.");
            }

            return SupportMapper.ToDto(ticket)!;
        }

        public async Task<IEnumerable<SupportTicketResponseDto>> GetTicketsByUserIdAsync(string userId)
        {
            var tickets = await _repository.GetByUserIdAsync(userId);
            return SupportMapper.ToDtoList(tickets);
        }

        public async Task<PaginatedResultDto<SupportTicketResponseDto>> GetAllTicketsPaginatedAsync(
            int page,
            int pageSize
        )
        {
            var (documents, total) = await _repository.GetAllPaginatedAsync(page, pageSize);

            var itemsDto = SupportMapper.ToDtoList(documents);

            return new PaginatedResultDto<SupportTicketResponseDto>(
                itemsDto,
                (int)total,
                page,
                pageSize
            );
        }

        public async Task UpdateTicketStatusAsync(string id, UpdateSupportTicketDto dto)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new ResourceNotFoundException("Ticket de suporte não encontrado.");
            }

            if (!string.IsNullOrEmpty(dto.Status)) ticket.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Priority)) ticket.Priority = dto.Priority;
            ticket.LastUpdated = DateTime.UtcNow;

            await _repository.UpdateAsync(id, ticket);
        }

        public async Task AssignTicketAsync(string id, string adminId)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new ResourceNotFoundException("Ticket de suporte não encontrado.");
            }

            ticket.AssignedTo = adminId;
            ticket.LastUpdated = DateTime.UtcNow;
            await _repository.UpdateAsync(id, ticket);
        }

        public async Task ReplyToTicketAsync(string id, string senderId, string senderRole, ReplyToTicketDto dto)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new ResourceNotFoundException("Ticket de suporte não encontrado.");
            }

            var response = new SupportResponse
            {
                SenderId = senderId,
                SenderRole = senderRole,
                Message = dto.Message,
                DateCreated = DateTime.UtcNow
            };

            // Se for admin respondendo, gerar evento Outbox para notificação por e-mail
            if (senderRole == "Admin")
            {
                using var session = await _mongoContext.StartSessionAsync();
                session.StartTransaction();
                try
                {
                    await _repository.AddResponseAsync(id, response);

                    var outboxEvent = new OutboxEvent
                    {
                        EventType = "TicketRepliedEvent",
                        Payload = JsonSerializer.Serialize(new { TicketId = id, StudentId = ticket.UserId, Message = dto.Message }),
                        CreatedAt = DateTime.UtcNow,
                        Processed = false
                    };

                    await _mongoContext.GetCollection<OutboxEvent>("OutboxEvents").InsertOneAsync(session, outboxEvent);

                    await session.CommitTransactionAsync();
                }
                catch
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
            else
            {
                await _repository.AddResponseAsync(id, response);
            }
        }
    }
}
