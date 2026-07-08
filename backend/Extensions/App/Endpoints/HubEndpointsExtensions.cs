using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.Shared.Infrastructure.Hubs;

namespace MeuCrudCsharp.Extensions.App.Endpoints;

public static class HubEndpointsExtensions
{
    public static WebApplication MapApplicationHubs(this WebApplication app)
    {
        app.MapHub<GlobalRealtimeHub>("/ws/realtime");
        app.MapHub<NotificationHub>("/hubs/notifications");

        return app;
    }
}