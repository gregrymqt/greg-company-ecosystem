using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol; // Namespace oficial do SDK MCP para .NET
using MeuCrudCsharp.Features.Mcp.Tools; // Pasta onde ficarão suas classes de ferramentas

namespace MeuCrudCsharp.Features.Mcp;

public static class McpFeatureExtensions
{
    /// <summary>
    /// Registra a infraestrutura do servidor MCP no container de Injeção de Dependência.
    /// </summary>
    public static IServiceCollection AddMcpContextServer(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Inicializa o servidor MCP informando os metadados do seu ecossistema
        services.AddMcpServer(options =>
        {
            options.ServerInfo = new ServerInfo(
                name: "greg-company-mcp-bridge",
                version: "1.0.0"
            );
        });

        // 2. Registra explicitamente as classes que contêm as ferramentas (Tools)
        // Isso permite que elas usem qualquer serviço injetado no container do .NET (ex: ApiDbContext)
        services.AddScoped<LogTools>();

        return services;
    }

    /// <summary>
    /// Mapeia os endpoints de transporte SSE para que o Antigravity/Cursor consiga se conectar.
    /// </summary>
    public static IEndpointRouteBuilder UseMcpEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // O SDK expõe nativamente rotas como:
        // GET  /mcp/sse (Para estabelecer a conexão contínua)
        // POST /mcp/message (Para envio de comandos JSON-RPC)
        endpoints.MapMcpEndpoints(options =>
        {
            options.RoutePrefix = "mcp"; // Define o prefixo das URLs
        });

        return endpoints;
    }
}