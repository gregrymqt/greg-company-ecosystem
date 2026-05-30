namespace MeuCrudCsharp.Features.Emails.ViewModels
{
    public class RefundConfirmationEmailViewModel
    {
        public string? UserName { get; set; }
        public string? PaymentId { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public string? AccountUrl { get; set; }
        public string? SiteUrl { get; set; }
    }
}
