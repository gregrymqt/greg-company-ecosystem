using MeuCrudCsharp.Features.Mcp.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;

namespace MeuCrudCsharp.Extensions.Services.Mcp;

public static class McpFeatureExtensions
{
    public static IServiceCollection AddMcpContextServer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddMcpServer(options =>
            {
                // O tipo correto de metadados do protocolo se chama Implementation
                options.ServerInfo = new Implementation
                {
                    Name = "greg-company-mcp-bridge",
                    Version = "1.0.0",
                };
            })
            .WithHttpTransport() // 💡 AQUI ESTÁ A CORREÇÃO CRÍTICA!
            .WithTools<LogTools>();

        return services;
    }

    public static IEndpointRouteBuilder UseMcpEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // O método correto de mapeamento da biblioteca é MapMcp
        endpoints.MapMcp("/mcp");

        return endpoints;
    }
}