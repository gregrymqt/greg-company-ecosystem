namespace MeuCrudCsharp.Features.Emails.ViewModels
{
    public class ConfirmationEmailViewModel
    {
        public string? UserName { get; set; }
        public string? PaymentId { get; set; }

        public string? AccessContentUrl { get; set; }
        public string? SiteUrl { get; set; }
    }
}
