using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace MeuCrudCsharp.Features.Mcp.Tools;

// 💡 1. Definição dos DTOs para blindar o payload do JSON-RPC
public class LogLinesArgs
{
    [Description("Quantidade de linhas para ler do log")]
    public int LinesCount { get; set; } = 50;
}

public class InfraLogArgs
{
    [Description("Quantidade de linhas para ler")]
    public int TailCount { get; set; } = 100;
}

public class LogTools
{
    private readonly bool _useKubernetes;

    public LogTools(IConfiguration configuration)
    {
        _useKubernetes = configuration.GetValue<bool>("USE_KUBERNETES_LOGS");
    }

    // ====================================================================
    // 1. LOGS DA APLICAÇÃO (SERILOG)
    // ====================================================================
    [McpServerTool(Name = "ReadLogsAsync")]
    [Description("Lê as últimas linhas do arquivo de log físico do Serilog (backend) para depurar exceções.")]
    public async Task<CallToolResult> ReadLogsAsync(LogLinesArgs args) // 💡 Ajustado para o DTO
    {
        if (_useKubernetes)
        {
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock 
                { 
                    Text = "A gravação de logs em disco local está desativada por diretrizes Stateless do Kubernetes (USE_KUBERNETES_LOGS=true). Por favor, consulte os logs diretamente no cluster utilizando a CLI nativa do console (ex: 'kubectl logs')." 
                }]
            };
        }

        var logPath = "log/log-.txt";

        if (!File.Exists(logPath))
        {
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = "Nenhum arquivo de log do Serilog foi encontrado no servidor." }]
            };
        }

        var lines = await File.ReadAllLinesAsync(logPath);
        int targetLines = args?.LinesCount ?? 50; // Fallback seguro
        var takeCount = Math.Min(lines.Length, targetLines);
        var lastLines = lines[^takeCount..];

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = string.Join("\n", lastLines) }],
        };
    }

    // ====================================================================
    // 2. LOGS DO SQL SERVER
    // ====================================================================
    [McpServerTool(Name = "ReadSqlServerLogsAsync")]
    [Description("Analisa os logs do SQL Server para investigar locks, timeouts ou erros de conexão.")]
    public async Task<CallToolResult> ReadSqlServerLogsAsync(InfraLogArgs args) // 💡 Ajustado para o DTO
    {
        int tail = args?.TailCount ?? 100;
        return await GetInfrastructureLogsAsync("mssql-db", tail);
    }

    // ====================================================================
    // 3. LOGS DO MONGODB
    // ====================================================================
    [McpServerTool(Name = "ReadMongoDbLogsAsync")]
    [Description("Analisa os logs do MongoDB para investigar queries lentas ou falhas de disco.")]
    public async Task<CallToolResult> ReadMongoDbLogsAsync(InfraLogArgs args) // 💡 Ajustado para o DTO
    {
        int tail = args?.TailCount ?? 100;
        return await GetInfrastructureLogsAsync("mongodb-store", tail);
    }

    // ====================================================================
    // 4. LOGS DO REDIS
    // ====================================================================
    [McpServerTool(Name = "ReadRedisLogsAsync")]
    [Description("Analisa os logs do Redis para investigar falhas de persistência ou despejo de memória.")]
    public async Task<CallToolResult> ReadRedisLogsAsync(InfraLogArgs args) // 💡 Ajustado para o DTO
    {
        int tail = args?.TailCount ?? 100;
        return await GetInfrastructureLogsAsync("redis-cache", tail);
    }

    // ====================================================================
    // MÉTODO AUXILIAR PRIVADO
    // ====================================================================
    private async Task<CallToolResult> GetInfrastructureLogsAsync(string appIdentifier, int tailCount)
    {
        try
        {
            string commandFileName = _useKubernetes ? "kubectl" : "docker";
            string commandArguments = _useKubernetes
                ? $"logs -l app={appIdentifier} -n greg-company --tail {tailCount}"
                : $"logs --tail {tailCount} {appIdentifier}";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = commandFileName,
                    Arguments = commandArguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var combinedLogs = output + "\n" + error;

            if (string.IsNullOrWhiteSpace(combinedLogs))
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"A infraestrutura '{appIdentifier}' está rodando mas não emitiu logs." }]
                };
            }

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = combinedLogs.Trim() }]
            };
        }
        catch (Exception ex)
        {
            string envName = _useKubernetes ? "Kubernetes" : "Docker";
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = $"Erro ao capturar logs do {envName} para {appIdentifier}: {ex.Message}" }]
            };
        }
    }
}