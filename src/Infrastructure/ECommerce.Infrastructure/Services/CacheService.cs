using System.Text.Json;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce.Infrastructure.Services;

public sealed class CacheService(IDistributedCache cache) : ICacheService, ISingletonDependency
{
    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await cache.GetStringAsync(key);
        return cachedValue is null ? default : JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.Add(expiration) });
    }
}
