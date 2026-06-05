using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace MeuCrudCsharp.Features.Mcp.Tools;

public class LogTools
{
    private readonly bool _useKubernetes;

    // Injetamos a configuração para ler a flag do .env
    public LogTools(IConfiguration configuration)
    {
        _useKubernetes = configuration.GetValue<bool>("USE_KUBERNETES_LOGS");
    }

    // ====================================================================
    // 1. LOGS DA APLICAÇÃO (SERILOG)
    // ====================================================================
    [Description(
        "Lê as últimas linhas do arquivo de log físico do Serilog (backend) para depurar exceções."
    )]
    public async Task<CallToolResult> ReadLogsAsync(
        [Description("Quantidade de linhas para ler do log")] int linesCount = 50
    )
    {
        var logPath = "log/log-.txt";

        if (!File.Exists(logPath))
        {
            return new CallToolResult
            {
                IsError = true,
                Content =
                [
                    new TextContentBlock
                    {
                        Text = "Nenhum arquivo de log do Serilog foi encontrado no servidor.",
                    },
                ],
            };
        }

        var lines = await File.ReadAllLinesAsync(logPath);
        var takeCount = Math.Min(lines.Length, linesCount);
        var lastLines = lines[^takeCount..];

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = string.Join("\n", lastLines) }],
        };
    }

    // ====================================================================
    // 2. LOGS DO SQL SERVER
    // ====================================================================
    [Description(
        "Analisa os logs do SQL Server para investigar locks, timeouts ou erros de conexão."
    )]
    public async Task<CallToolResult> ReadSqlServerLogsAsync(
        [Description("Quantidade de linhas para ler")] int tailCount = 100
    )
    {
        // O identificador "mssql-db" serve tanto para o container Docker quanto para a Label do Kubernetes!
        return await GetInfrastructureLogsAsync("mssql-db", tailCount);
    }

    // ====================================================================
    // 3. LOGS DO MONGODB
    // ====================================================================
    [Description("Analisa os logs do MongoDB para investigar queries lentas ou falhas de disco.")]
    public async Task<CallToolResult> ReadMongoDbLogsAsync(
        [Description("Quantidade de linhas para ler")] int tailCount = 100
    )
    {
        return await GetInfrastructureLogsAsync("mongodb-store", tailCount);
    }

    // ====================================================================
    // 4. LOGS DO REDIS
    // ====================================================================
    [Description(
        "Analisa os logs do Redis para investigar falhas de persistência ou despejo de memória."
    )]
    public async Task<CallToolResult> ReadRedisLogsAsync(
        [Description("Quantidade de linhas para ler")] int tailCount = 100
    )
    {
        return await GetInfrastructureLogsAsync("redis-cache", tailCount);
    }

    // ====================================================================
    // MÉTODO AUXILIAR PRIVADO (O CÉREBRO DA INTEGRAÇÃO)
    // ====================================================================
    private async Task<CallToolResult> GetInfrastructureLogsAsync(
        string appIdentifier,
        int tailCount
    )
    {
        try
        {
            // A Mágica do Booleano acontece aqui:
            string commandFileName = _useKubernetes ? "kubectl" : "docker";

            // Se for K8s, usamos o seletor de label (-l app=...) no namespace greg-company
            // Se for Docker, usamos o comando padrão de container
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
                    Content =
                    [
                        new TextContentBlock
                        {
                            Text =
                                $"A infraestrutura '{appIdentifier}' está rodando mas não emitiu logs.",
                        },
                    ],
                };
            }

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = combinedLogs.Trim() }],
            };
        }
        catch (Exception ex)
        {
            string envName = _useKubernetes ? "Kubernetes" : "Docker";
            return new CallToolResult
            {
                IsError = true,
                Content =
                [
                    new TextContentBlock
                    {
                        Text =
                            $"Erro ao capturar logs do {envName} para {appIdentifier}: {ex.Message}",
                    },
                ],
            };
        }
    }
}
