using System.Text.Json;
using System.Text.RegularExpressions;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public sealed class HybridCacheManager : ICacheManager, ISingletonDependency
{
    private readonly IMemoryCache _memoryCache; // L1 Cache
    private readonly IDistributedCache _distributedCache; // L2 Cache
    private readonly ILogger<HybridCacheManager> _logger;

    public HybridCacheManager(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCacheManager> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit in L1 cache for key: {Key}", key);
            return cachedValue;
        }

        var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
            
            // Populate L1 cache
            _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
            _logger.LogDebug("Cache hit in L2 cache for key: {Key}, populated L1 cache", key);
            
            return deserializedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var defaultExpiration = expiration ?? TimeSpan.FromHours(1);
        
        _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
        
        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = defaultExpiration }, 
            cancellationToken);

        _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, defaultExpiration);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key, cancellationToken);
        
        _logger.LogDebug("Removed cache entry for key: {Key}", key);
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_memoryCache is MemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is object coherentState)
            {
                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(coherentState);

                if (entriesCollection is System.Collections.IDictionary entries)
                {
                    var keysToRemove = new List<object>();
                    var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    foreach (System.Collections.DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString() != null && regex.IsMatch(entry.Key.ToString()!))
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _memoryCache.Remove(key);
                    }
                }
            }
        }

            _logger.LogWarning("Pattern-based removal for distributed cache is not implemented. Pattern: {Pattern}", pattern);
        
        _logger.LogDebug("Removed cache entries matching pattern: {Pattern}", pattern);
        
        return Task.CompletedTask;
    }
} 