using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;

namespace MeuCrudCsharp.Extensions.App.Endpoints;

public static class HubEndpointsExtensions
{
    public static WebApplication MapApplicationHubs(this WebApplication app)
    {
        app.MapHub<GlobalRealtimeHub>("/ws/realtime");

        return app;
    }
}