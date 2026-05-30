using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Emails.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
