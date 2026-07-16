using System;
using System.Text.Json;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Caching;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MeuCrudCsharp.Features.Caching.Application.Services;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpireTime = null
    )
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            logger.LogDebug("Cache HIT para a chave {CacheKey}.", key);
            return cachedValue;
        }

        logger.LogDebug("Cache MISS para a chave {CacheKey}. Buscando da fonte de dados.", key);

        var freshValue = await factory();

        if (freshValue != null)
        {
            await SetAsync(key, freshValue, absoluteExpireTime);
        }

        return freshValue;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
            logger.LogInformation("Chave de cache {CacheKey} removida com sucesso.", key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Falha ao remover a chave de cache {CacheKey}. O Redis pode estar indisponível.",
                key
            );
        }
    }

    public async Task InvalidateCacheByKeyAsync(string cacheVersionKey)
    {
        try
        {
            var newVersion = Guid.NewGuid().ToString();
            await SetAsync(cacheVersionKey, newVersion, TimeSpan.FromDays(30));

            logger.LogInformation(
                "Cache para a chave de versão '{CacheKey}' invalidado. Nova versão: {CacheVersion}",
                cacheVersionKey,
                newVersion
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao invalidar o cache para a chave de versão {CacheKey}",
                cacheVersionKey
            );
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "Desserialização falhou para a chave {CacheKey} (possível dado residual ou schema antigo). Limpando a chave.",
                key
            );
            await RemoveAsync(key);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Falha ao ler o cache para a chave {CacheKey}. Redis pode estar indisponível.",
                key
            );
            return default;
        }
    }

    public async Task<long> IncrementAsync(string key, int expirationSeconds)
    {
        long count = 0;

        var valueStr = await cache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(valueStr) && long.TryParse(valueStr, out var current))
        {
            count = current;
        }

        count++;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds),
        };

        await cache.SetStringAsync(key, count.ToString(), options);

        return count;
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? DefaultExpiration,
            };
            var serializedValue = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, serializedValue, options);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível conectar ao Redis ou serializar para definir a chave {CacheKey}.",
                key
            );
        }
    }
}
