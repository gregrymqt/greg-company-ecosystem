using Microsoft.AspNetCore.Identity;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities; // Temp for Payments and Subscription

namespace MeuCrudCsharp.Features.Auth.Domain.Entities;

public class Users : AspNetCore.Identity.Mongo.Model.MongoUser<string>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }

    // --- IMAGENS ---
    public string? AvatarFileId { get; set; }
    public string? AvatarUrl { get; set; }

    // --- Autenticação Externa ---
    public string? GoogleId { get; set; }
    public string? CustomerId { get; set; }

    // --- Relacionamentos ---
    public virtual Subscription? Subscription { get; set; }
    public virtual ICollection<Payments> Payments { get; set; } = new List<Payments>();
}


