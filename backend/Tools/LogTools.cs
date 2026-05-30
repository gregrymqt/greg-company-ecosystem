using ModelContextProtocol;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Mcp.Tools;

public class LogTools
{
    // ====================================================================
    // FERRAMENTA 1: LOGS DA APLICAÇÃO (SERILOG)
    // ====================================================================
    [McpTool(Name = "read_api_logs", Description = "Lê as últimas linhas do arquivo de log físico do Serilog (backend) para depurar exceções.")]
    public async Task<McpToolResponse> ReadLogsAsync(int linesCount = 50)
    {
        var logPath = "log/log-.txt";

        if (!File.Exists(logPath))
            return new McpToolResponse("Nenhum arquivo de log do Serilog foi encontrado no servidor.");

        var lines = await File.ReadAllLinesAsync(logPath);
        var takeCount = Math.Min(lines.Length, linesCount);
        
        // Operador ^ faz o "tail -n" nativamente no C#
        var lastLines = lines[^takeCount..]; 

        return new McpToolResponse(string.Join("\n", lastLines));
    }

    // ====================================================================
    // FERRAMENTA 2: LOGS DO SQL SERVER (DOCKER)
    // ====================================================================
    [McpTool(Name = "read_sqlserver_logs", Description = "Analisa os logs do container docker do SQL Server para investigar locks, timeouts ou erros de conexão.")]
    public async Task<McpToolResponse> ReadSqlServerLogsAsync(int tailCount = 100)
    {
        // Usa o nome exato do container definido no seu docker-compose.yml
        return await GetDockerLogsAsync("mssql-db", tailCount); 
    }

    // ====================================================================
    // FERRAMENTA 3: LOGS DO MONGODB (DOCKER)
    // ====================================================================
    [McpTool(Name = "read_mongodb_logs", Description = "Analisa os logs do container docker do MongoDB para investigar queries lentas ou falhas de disco.")]
    public async Task<McpToolResponse> ReadMongoDbLogsAsync(int tailCount = 100)
    {
        // Usa o nome exato do container definido no seu docker-compose.yml
        return await GetDockerLogsAsync("mongodb-store", tailCount);
    }

    // ====================================================================
    // FERRAMENTA 4: LOGS DO REDIS (DOCKER)
    // ====================================================================
    [McpTool(Name = "read_redis_logs", Description = "Analisa os logs do container docker do Redis para investigar falhas de persistência AOF ou despejo de memória.")]
    public async Task<McpToolResponse> ReadRedisLogsAsync(int tailCount = 100)
    {
        // Usa o nome exato do container definido no seu docker-compose.yml
        return await GetDockerLogsAsync("redis-cache", tailCount);
    }

    // ====================================================================
    // MÉTODO AUXILIAR PRIVADO (Não vira ferramenta para a IA, só ajuda o código)
    // ====================================================================
    private async Task<McpToolResponse> GetDockerLogsAsync(string containerName, int tailCount)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"logs --tail {tailCount} {containerName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true, // Docker as vezes joga erro no stderr
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            // Lê as saídas padrão e de erro do container
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            var combinedLogs = output + "\n" + error;

            if (string.IsNullOrWhiteSpace(combinedLogs))
                return new McpToolResponse($"Container '{containerName}' está rodando mas não emitiu logs, ou o container não foi encontrado.");

            return new McpToolResponse(combinedLogs.Trim());
        }
        catch (Exception ex)
        {
            return new McpToolResponse($"Erro ao tentar capturar logs do container {containerName}. Certifique-se de que a API tem permissões para executar o binário do Docker. Erro: {ex.Message}");
        }
    }
}