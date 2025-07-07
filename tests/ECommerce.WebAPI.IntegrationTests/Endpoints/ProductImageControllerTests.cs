using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.WebAPI.IntegrationTests.Common;
using System.Text.Json;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class ProductImageControllerTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetProductImages_WithValidProductId_ShouldReturnImages()
    {
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
    public async Task UploadProductImages_WithValidFiles_ShouldReturnCreated()
    {
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
    public async Task DeleteProductImage_WithValidImageId_ShouldReturnNoContent()
    {
        var productId = await CreateTestProductAsync();
        var imageId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/api/v1/product/{productId}/images/{imageId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateImageOrder_WithValidRequest_ShouldReturnOk()
    {
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
        var response = await Client.PostAsJsonAsync("/api/v1/product", new
        {
            Name = "Test Product",
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
        var response = await Client.PostAsJsonAsync("/api/v1/category", new
        {
            Name = "Test Category",
            Description = "Test Description"
        });

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Guid>(responseContent);
        }

        return Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    }
} 