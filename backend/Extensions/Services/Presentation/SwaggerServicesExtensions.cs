namespace MeuCrudCsharp.Extensions.Services.Presentation;

public static class SwaggerServicesExtensions
{
    public static WebApplicationBuilder AddSwaggerServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }
}
