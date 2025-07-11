using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.WebAPI.IntegrationTests.Common;
using System.Text.Json;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class ProductImageControllerTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task InitializeAsync() => await ResetDatabaseAsync();
    [Fact]
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductImages_WithValidProductId_ShouldReturnImages()
    {
        await ResetDatabaseAsync();
        var productId = await CreateTestProductAsync();

        var response = await Client.GetAsync($"/api/v1/product/{productId}/images");

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var images = JsonSerializer.Deserialize<List<ProductImageResponseDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        images.Should().NotBeNull();
        images.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductImages_WithFilterByImageType_ShouldReturnFilteredImages()
    {
        await ResetDatabaseAsync();
        var productId = await CreateTestProductAsync();

        var response = await Client.GetAsync($"/api/v1/product/{productId}/images?imageType={ImageType.Main}");

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var images = JsonSerializer.Deserialize<List<ProductImageResponseDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        images.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadProductImages_WithValidFiles_ShouldReturnBadRequest()
    {
        await ResetDatabaseAsync();
        var productId = await CreateTestProductAsync();

        using var form = new MultipartFormDataContent();
        
        var imageContent = CreateTestImageContent();
        form.Add(imageContent, "Images[0].File", "test.jpg");
        form.Add(new StringContent(ImageType.Main.ToString()), "Images[0].ImageType");
        form.Add(new StringContent("1"), "Images[0].DisplayOrder");
        form.Add(new StringContent("Test alt text"), "Images[0].AltText");

        var response = await Client.PostAsync($"/api/v1/product/{productId}/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProductImage_WithValidImageId_ShouldReturnBadRequest()
    {
        await ResetDatabaseAsync();
        var productId = await CreateTestProductAsync();
        var imageId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/api/v1/product/{productId}/images/{imageId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateImageOrder_WithValidRequest_ShouldReturnOk()
    {
        await ResetDatabaseAsync();
        var productId = await CreateTestProductAsync();
        
        var request = new UpdateImageOrderRequest(new Dictionary<Guid, int>
        {
            { Guid.NewGuid(), 1 },
            { Guid.NewGuid(), 2 }
        });

        var response = await Client.PutAsJsonAsync($"/api/v1/product/{productId}/images/reorder", request);

        response.EnsureSuccessStatusCode();
    }

    private static ByteArrayContent CreateTestImageContent()
    {
        var imageBytes = new byte[1024];
        Random.Shared.NextBytes(imageBytes);
        return new ByteArrayContent(imageBytes) { Headers = { { "Content-Type", "image/jpeg" } } };
    }

    private async Task<Guid> CreateTestProductAsync()
    {
        var productName = $"Test Product {Guid.NewGuid()}";
        var response = await Client.PostAsJsonAsync("/api/v1/product", new
        {
            Name = productName,
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = await CreateTestCategoryAsync(),
            StockQuantity = 10
        });

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Guid>(responseContent);
    }

    private async Task<Guid> CreateTestCategoryAsync()
    {
        var categoryName = $"Test Category {Guid.NewGuid()}";
        var response = await Client.PostAsJsonAsync("/api/v1/category", new
        {
            Name = categoryName
        });

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Guid>(responseContent);
    }
} 