namespace MeuCrudCsharp.Extensions.Services;

public static class FeatureDiscoveryExtensions
{
    public static WebApplicationBuilder AddFeatureDiscovery(this WebApplicationBuilder builder)
    {
        builder.Services.Scan(scan =>
            scan.FromEntryAssembly()
                .AddClasses(classes =>
                    classes
                        // Filtra apenas classes que residem dentro do ecossistema de Features ou Shared
                        .Where(type =>
                            type.Namespace != null
                            && (
                                type.Namespace.StartsWith("MeuCrudCsharp.Features")
                                || type.Namespace.StartsWith("MeuCrudCsharp.Features.Shared")
                            )
                        )
                        // Aplica a convenção de nomenclatura da Clean Architecture / Vertical Slices
                        .Where(type =>
                            type.Name.EndsWith("Service")
                            || type.Name.EndsWith("Repository")
                            || type.Name.EndsWith("Integration")
                            || type.Name.EndsWith("Notification")
                            || type.Namespace.EndsWith("Work")
                        )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return builder;
    }
}
