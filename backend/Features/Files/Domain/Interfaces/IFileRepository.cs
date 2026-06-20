using System;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;

namespace MeuCrudCsharp.Features.Files.Domain.Interfaces;

public interface IFileRepository
{
    Task<EntityFile?> GetByIdAsync(string id);
    Task AddAsync(EntityFile arquivo);
    Task UpdateAsync(EntityFile arquivo);
    Task DeleteAsync(EntityFile arquivo);
}


