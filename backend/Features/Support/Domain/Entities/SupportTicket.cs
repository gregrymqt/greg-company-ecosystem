using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.Support.Domain.Entities
{
    public class SupportTicket
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = "open";
        public string? AssignedTo { get; set; }
        public List<SupportResponse> Responses { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
