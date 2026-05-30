using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.AspNetCore;
using MeuCrudCsharp.Features.Mcp.Tools;

namespace MeuCrudCsharp.Extensions.Mcp;

public static class McpFeatureExtensions
{
    public static IServiceCollection AddMcpContextServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMcpServer(options =>
        {
            // O tipo correto de metadados do protocolo se chama Implementation
            options.ServerInfo = new Implementation
            {
                Name = "greg-company-mcp-bridge",
                Version = "1.0.0"
            };
        });

        services.AddScoped<LogTools>();

        return services;
    }

    public static IEndpointRouteBuilder UseMcpEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // O método correto de mapeamento da biblioteca é MapMcp
        var mcpGroup = endpoints.MapMcp("/mcp");

        /* mcpGroup.MapMcpTool(
            name: "read_api_logs",
            description: "Lê as últimas linhas do arquivo de log físico do Serilog (backend) para depurar exceções.",
            async (int linesCount, LogTools tools) => await tools.ReadLogsAsync(linesCount)
        ); */

        /* mcpGroup.MapMcpTool(
            name: "read_sqlserver_logs",
            description: "Analisa os logs do container docker do SQL Server para investigar locks, timeouts ou erros de conexão.",
            async (int tailCount, LogTools tools) => await tools.ReadSqlServerLogsAsync(tailCount)
        ); */

        /* mcpGroup.MapMcpTool(
            name: "read_mongodb_logs",
            description: "Analisa os logs do container docker do MongoDB para investigar queries lentas ou falhas de disco.",
            async (int tailCount, LogTools tools) => await tools.ReadMongoDbLogsAsync(tailCount)
        ); */

        /* mcpGroup.MapMcpTool(
            name: "read_redis_logs",
            description: "Analisa os logs do container docker do Redis para investigar falhas de persistência ou despejo de memória.",
            async (int tailCount, LogTools tools) => await tools.ReadRedisLogsAsync(tailCount)
        ); */

        return endpoints;
    }
}