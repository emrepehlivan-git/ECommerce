using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class HybridCacheManagerTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly HybridCacheManager _cacheManager;
    private readonly ILogger<HybridCacheManager> _logger;

    public HybridCacheManagerTests()
    {
        _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _logger = new Mock<ILogger<HybridCacheManager>>().Object;
        _cacheManager = new HybridCacheManager(_memoryCache, _distributedCache, _logger);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFromL1Cache_WhenValueExistsInMemoryCache()
    {
        // Arrange
        var key = "test-key-l1";
        var value = new TestRecord("L1 Value");
        _memoryCache.Set(key, value);

        // Act
        var result = await _cacheManager.GetAsync<TestRecord>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(value.Value);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFromL2Cache_WhenValueNotInL1ButInL2()
    {
        // Arrange
        var key = "test-key-l2";
        var value = new TestRecord("L2 Value");
        await _cacheManager.SetAsync(key, value, TimeSpan.FromMinutes(1));
        
        // Clear L1 cache to simulate L1 miss
        _memoryCache.Remove(key);

        // Act
        var result = await _cacheManager.GetAsync<TestRecord>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(value.Value);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreInBothCaches()
    {
        // Arrange
        var key = "test-set-key";
        var value = new TestRecord("Test Value");

        // Act
        await _cacheManager.SetAsync(key, value, TimeSpan.FromMinutes(1));

        // Assert
        // Check L1 cache
        _memoryCache.TryGetValue(key, out var l1Result).Should().BeTrue();
        ((TestRecord)l1Result!).Value.Should().Be(value.Value);

        // Check L2 cache
        var l2Result = await _distributedCache.GetStringAsync(key);
        l2Result.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveFromBothCaches()
    {
        // Arrange
        var key = "remove-key";
        var value = new TestRecord("Remove Value");
        await _cacheManager.SetAsync(key, value, TimeSpan.FromMinutes(1));

        // Act
        await _cacheManager.RemoveAsync(key);

        // Assert
        var result = await _cacheManager.GetAsync<TestRecord>(key);
        result.Should().BeNull();
        
        _memoryCache.TryGetValue(key, out _).Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheManager.GetAsync<TestRecord>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldUseDefaultExpiration_WhenExpirationNotProvided()
    {
        // Arrange
        var key = "default-expiration-key";
        var value = new TestRecord("Default Expiration");

        // Act
        await _cacheManager.SetAsync(key, value); // No expiration provided

        // Assert
        var result = await _cacheManager.GetAsync<TestRecord>(key);
        result.Should().NotBeNull();
    }

    private record TestRecord(string Value);
}
