namespace MeuCrudCsharp.Features.Emails.Application.ViewModels
{
    public class RejectionEmailViewModel
    {
        public required string UserName { get; set; }
        public required string PaymentId { get; set; }

        public required string PaymentPageUrl { get; set; }
        public required string SiteUrl { get; set; }
    }
}
