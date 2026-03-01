using System.Security.Claims;
using System.Threading.Tasks;
using MercadoPago.Resource.Preference;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces
{
    public interface IPreferencePaymentService
    {
        Task<string> CreatePreferenceAsync(CreatePreferenceDto model);
    }
}
