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
                Context = document.Context,
                Explanation = document.Explanation,
                Status = document.Status,
                CreatedAt = document.CreatedAt,
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
