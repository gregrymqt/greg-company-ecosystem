using MeuCrudCsharp.Features.Support.Domain.Entities;
using MeuCrudCsharp.Features.Support.Application.DTOs;

namespace MeuCrudCsharp.Features.Support.Application.Utils
{
    public static class SupportMapper
    {
        public static SupportTicketResponseDto? ToDto(SupportTicket? document)
        {
            if (document == null)
                return null;

            return new SupportTicketResponseDto
            {
                Id = document.Id,
                UserId = document.UserId,
                Title = document.Title,
                Category = document.Category,
                Priority = document.Priority,
                Status = document.Status,
                AssignedTo = document.AssignedTo,
                CreatedAt = document.CreatedAt,
                LastUpdated = document.LastUpdated,
                Responses = document.Responses?.Select(r => new SupportResponseDto
                {
                    SenderId = r.SenderId,
                    SenderRole = r.SenderRole,
                    Message = r.Message,
                    DateCreated = r.DateCreated
                }).ToList() ?? new List<SupportResponseDto>()
            };
        }

        public static List<SupportTicketResponseDto> ToDtoList(
            IEnumerable<SupportTicket> documents
        )
        {
            if (documents == null)
                return new List<SupportTicketResponseDto>();

            return documents.Select(ToDto).ToList();
        }
    }
}
