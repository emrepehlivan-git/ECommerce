using System.Collections.Concurrent;
using System.Text.Json;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Services;

public sealed class CacheManager(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer) : ICacheManager, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, object> _cacheKeys = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cacheKeys.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        var distributedValue = await distributedCache.GetStringAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue)!;
            _cacheKeys.TryAdd(key, deserializedValue);
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
        _cacheKeys[key] = value!;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
        _cacheKeys.TryRemove(key, out _);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);

        foreach (var key in keys)
        {
            await RemoveAsync(key.ToString(), cancellationToken);
        }
    }
} 