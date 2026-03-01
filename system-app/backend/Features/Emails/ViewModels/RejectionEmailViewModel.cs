namespace MeuCrudCsharp.Features.Emails.ViewModels
{
    public class RejectionEmailViewModel
    {
        public required string UserName { get; set; }
        public required string PaymentId { get; set; }

        /// <summary>
        /// URL para a página de pagamento para o usuário tentar novamente.
        /// </summary>
        public required string PaymentPageUrl { get; set; }

        /// <summary>
        /// URL principal do seu site (para o logo e rodapé).
        /// </summary>
        public required string SiteUrl { get; set; }
    }
}
