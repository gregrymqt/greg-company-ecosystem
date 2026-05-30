using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Emails.Application.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
