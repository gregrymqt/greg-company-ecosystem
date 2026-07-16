using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Support.Application.DTOs
{
    public class CreateSupportTicketDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "A prioridade é obrigatória.")]
        public string Priority { get; set; } = string.Empty;

        [Required(ErrorMessage = "A mensagem inicial é obrigatória.")]
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateSupportTicketDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }

    public class SupportTicketResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
        public List<SupportResponseDto> Responses { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SupportResponseDto
    {
        public Guid SenderId { get; set; }
        public string SenderRole { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        
    }

    public class ReplyToTicketDto
    {
        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        public string Message { get; set; } = string.Empty;
    }
}