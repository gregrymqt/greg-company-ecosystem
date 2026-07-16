using Microsoft.AspNetCore.Identity;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;

namespace MeuCrudCsharp.Features.Auth.Domain.Entities;

public class Users : IdentityUser<Guid>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }

    public string? AvatarFileId { get; set; }
    public string? AvatarUrl { get; set; }

    public string? GoogleId { get; set; }
    public string? CustomerId { get; set; }

    public List<string> Tenants { get; set; } = new List<string>();

    public virtual Subscription? Subscription { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
