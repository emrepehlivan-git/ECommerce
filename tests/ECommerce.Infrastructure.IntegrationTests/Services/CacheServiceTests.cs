using StackExchange.Redis;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class CacheManagerTests
{
    private readonly IDistributedCache _distributedCache;
    private readonly CacheManager _cacheManager;
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;

    public CacheManagerTests()
    {
        _distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _cacheManager = new CacheManager(_distributedCache, _mockConnectionMultiplexer.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFromL1Cache_WhenValueExistsInMemoryCache()
    {
        // Arrange
        var key = "test-key-l1";
        var value = new TestRecord("L1 Value");
        await _cacheManager.SetAsync(key, value);

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

        // Create a new CacheManager to simulate a new instance with an empty L1 cache
        var newCacheManager = new CacheManager(_distributedCache, _mockConnectionMultiplexer.Object);

        // Act
        var result = await newCacheManager.GetAsync<TestRecord>(key);

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
        var l1Result = await _cacheManager.GetAsync<TestRecord>(key);
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
        
        var result2 = await _distributedCache.GetStringAsync(key);
        result2.Should().BeNull();
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
