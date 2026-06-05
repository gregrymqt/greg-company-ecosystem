namespace MeuCrudCsharp.Extensions.Services.Presentation;

public static class SignalRServicesExtensions
{
    public static WebApplicationBuilder AddSignalRServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR();

        return builder;
    }
}
