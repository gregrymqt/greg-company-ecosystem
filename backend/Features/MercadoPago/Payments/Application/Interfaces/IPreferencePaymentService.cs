using System.Security.Claims;
using System.Threading.Tasks;
using MercadoPago.Resource.Preference;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces
{
    public interface IPreferencePaymentService
    {
        Task<string> CreatePreferenceAsync(CreatePreferenceDto model);
    }
}
