using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;

namespace MeuCrudCsharp.Extensions.App.Endpoints;

public static class HubEndpointsExtensions
{
    public static WebApplication MapApplicationHubs(this WebApplication app)
    {
        app.MapHub<VideoProcessingHub>("/videoProcessingHub");
        app.MapHub<RefundProcessingHub>("/RefundProcessingHub");
        app.MapHub<PaymentProcessingHub>("/PaymentProcessingHub");

        return app;
    }
}