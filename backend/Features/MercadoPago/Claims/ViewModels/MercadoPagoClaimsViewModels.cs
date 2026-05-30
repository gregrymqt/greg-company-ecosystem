using System;
using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.ViewModels;

public class MercadoPagoClaimsViewModels
{
    public class ClaimsIndexViewModel
    {
        public List<ClaimSummaryViewModel> Claims { get; set; } = new();

        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class ClaimSummaryViewModel
    {
        public int InternalId { get; set; }

        public long MpClaimId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        public string? ResourceId { get; set; }

        public string? Type { get; set; }
        public string? Status { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsUrgent { get; set; }
    }

    public class ClaimDetailViewModel
    {
        public int InternalId { get; set; }
        public long MpClaimId { get; set; }

        public string? Status { get; set; }
        public string? Stage { get; set; }
        public string? PlayerRole { get; set; }

        public List<ClaimMessageViewModel> Messages { get; set; } = new();

        public bool CanReply => Status != "closed";
    }

    public class ClaimMessageViewModel
    {
        public string? MessageId { get; set; }

        public string? SenderRole { get; set; }

        public string? Content { get; set; }

        public DateTime DateCreated { get; set; }

        public List<string> Attachments { get; set; } = new();

        public bool IsMe { get; set; }
        public bool IsMediator => SenderRole == "mediator";
    }

    public class ReplyClaimViewModel
    {
        [Required]
        public long InternalId { get; set; }

        [Required(ErrorMessage = "A mensagem não pode ser vazia.")]
        [MinLength(5, ErrorMessage = "Escreva uma resposta mais detalhada.")]
        public string? Message { get; set; }

        public List<string>? Attachments { get; set; }
    }
}
