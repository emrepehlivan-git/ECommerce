using System.Text.Json;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Services;

public sealed class CacheManager(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer) : ICacheManager, ISingletonDependency
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var distributedValue = await distributedCache.GetStringAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue)!;
            return deserializedValue;
        }
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var defaultExpiration = expiration ?? TimeSpan.FromHours(1);
        var serializedValue = JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, serializedValue, 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = defaultExpiration }, 
            cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var endpoints = connectionMultiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = connectionMultiplexer.GetServer(endpoint);
            if (!server.IsConnected) continue;
            var batchSize = 500;
            var keys = server.Keys(pattern: pattern, pageSize: batchSize);
            var keyList = keys.Select(k => k.ToString()).ToList();
            foreach (var key in keyList)
            {
                await distributedCache.RemoveAsync(key, cancellationToken);
            }
        }
    }
} 