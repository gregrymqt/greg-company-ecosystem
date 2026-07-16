using System;

namespace MeuCrudCsharp.Features.Support.Domain.Entities
{
    public class SupportResponse
    {
        public string SenderId { get; set; } = null!;
        public string SenderRole { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
