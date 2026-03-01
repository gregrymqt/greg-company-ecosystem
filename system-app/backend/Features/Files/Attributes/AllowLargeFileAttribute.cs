using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeuCrudCsharp.Features.Files.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowLargeFileAttribute : RequestFormLimitsAttribute, IResourceFilter
{
    private readonly long _maxSizeBytes;

    public AllowLargeFileAttribute(int maxSizeInMb = 500)
    {
        if (maxSizeInMb <= 0)
            throw new ArgumentException(
                "O tamanho máximo deve ser maior que zero.",
                nameof(maxSizeInMb)
            );

        if (maxSizeInMb > 5120)
            throw new ArgumentException(
                "O tamanho máximo não pode exceder 5GB (5120MB).",
                nameof(maxSizeInMb)
            );

        _maxSizeBytes = (long)maxSizeInMb * 1024 * 1024;

        this.MultipartBodyLengthLimit = _maxSizeBytes;
        this.ValueLengthLimit = int.MaxValue;
        this.ValueCountLimit = int.MaxValue; // Permite muitos campos no formulário
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var features = context.HttpContext.Features;
        var maxBodySizeFeature = features.Get<IHttpMaxRequestBodySizeFeature>();

        if (maxBodySizeFeature is { IsReadOnly: false })
        {
            maxBodySizeFeature.MaxRequestBodySize = _maxSizeBytes;
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
