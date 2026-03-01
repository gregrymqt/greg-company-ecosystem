namespace MeuCrudCsharp.Features.Emails.ViewModels
{
    public class ChargebackReceivedEmailViewModel
    {
        public string UserName { get; }
        public long ChargebackId { get; }
        public string SupportUrl { get; }

        public ChargebackReceivedEmailViewModel(
            string userName,
            long chargebackId,
            string supportUrl
        )
        {
            UserName = userName;
            ChargebackId = chargebackId;
            SupportUrl = supportUrl;
        }
    }
}