using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Support.DTOs
{
    public class CreateSupportTicketDto
    {
        [Required(ErrorMessage = "O contexto é obrigatório.")]
        [MinLength(3, ErrorMessage = "O contexto deve ter no mínimo 3 caracteres.")]
        public string? Context { get; set; } = string.Empty;

        [Required(ErrorMessage = "A explicação é obrigatória.")]
        [MinLength(10, ErrorMessage = "A explicação deve ter no mínimo 10 caracteres.")]
        public string? Explanation { get; set; } = string.Empty;
    }

    public class UpdateSupportTicketDto
    {
        [Required]
        public string? Status { get; set; }
    }

    public class SupportTicketResponseDto
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Context { get; set; }
        public string? Explanation { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
