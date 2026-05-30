using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Mcp.Tools;

public class LogTools
{
    // ====================================================================
    // 1. LOGS DA APLICAÇÃO (SERILOG)
    // ====================================================================
    public async Task<CallToolResult> ReadLogsAsync(int linesCount = 50)
    {
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
        var takeCount = Math.Min(lines.Length, linesCount);
        var lastLines = lines[^takeCount..];

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = string.Join("\n", lastLines) }]
        };
    }

    // ====================================================================
    // 2. LOGS DO SQL SERVER (DOCKER)
    // ====================================================================
    public async Task<CallToolResult> ReadSqlServerLogsAsync(int tailCount = 100)
    {
        return await GetDockerLogsAsync("mssql-db", tailCount);
    }

    // ====================================================================
    // 3. LOGS DO MONGODB (DOCKER)
    // ====================================================================
    public async Task<CallToolResult> ReadMongoDbLogsAsync(int tailCount = 100)
    {
        return await GetDockerLogsAsync("mongodb-store", tailCount);
    }

    // ====================================================================
    // 4. LOGS DO REDIS (DOCKER)
    // ====================================================================
    public async Task<CallToolResult> ReadRedisLogsAsync(int tailCount = 100)
    {
        return await GetDockerLogsAsync("redis-cache", tailCount);
    }

    // ====================================================================
    // MÉTODO AUXILIAR PRIVADO
    // ====================================================================
    private async Task<CallToolResult> GetDockerLogsAsync(string containerName, int tailCount)
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
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
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
                    Content = [new TextContentBlock { Text = $"Container '{containerName}' está rodando mas não emitiu logs." }]
                };
            }

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = combinedLogs.Trim() }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = $"Erro ao capturar logs do container {containerName}: {ex.Message}" }]
            };
        }
    }
}