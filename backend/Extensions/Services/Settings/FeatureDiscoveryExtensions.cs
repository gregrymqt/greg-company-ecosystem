namespace MeuCrudCsharp.Extensions.Services;

public static class FeatureDiscoveryExtensions
{
    public static WebApplicationBuilder AddFeatureDiscovery(this WebApplicationBuilder builder)
    {
        builder.Services.Scan(scan =>
            scan.FromAssemblies(typeof(FeatureDiscoveryExtensions).Assembly)
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
                            || type.Name.EndsWith("UnitOfWork")
                            || type.Name.EndsWith("Renderer")
                            || type.Name.EndsWith("Hub")
                        )
                        .Where(type => type.Name != "SendGridEmailSenderService")
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return builder;
    }
}
