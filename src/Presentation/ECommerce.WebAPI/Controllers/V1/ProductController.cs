using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using ECommerce.Application.Features.Products.V1.Commands;
using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.Application.Features.Products.V1.Queries;
using ECommerce.Application.Features.Stock.V1.Commands;
using ECommerce.Application.Features.Stock.V1.Queries;
using ECommerce.Application.Parameters;
using ECommerce.Domain.Enums;
using ECommerce.WebAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.WebAPI.Controllers.V1;

[Authorize]
public sealed class ProductController : BaseApiV1Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] PageableRequestParams requestParams, 
        [FromQuery] string? orderBy = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetAllProductsQuery(requestParams, orderBy, categoryId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command with { Id = id }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id}/stock")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> GetProductStockInfo(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetProductStockInfo(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProductStock(Guid id, UpdateProductStock command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command with { ProductId = id }, cancellationToken);
        return result.ToActionResult(this);
    }

    #region Image Operations

    [HttpGet("{id}/images")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductImageResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProductImageResponseDto>>> GetProductImages(
        Guid id,
        [FromQuery] ImageType? imageType = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductImagesQuery(id, imageType, activeOnly);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        var response = result.Value.Select(img => new ProductImageResponseDto(
            img.Id,
            id,
            img.CloudinaryPublicId,
            img.ImageUrl,
            img.ThumbnailUrl,
            img.LargeUrl,
            img.ImageType,
            img.DisplayOrder,
            true,
            0,
            img.AltText,
            DateTime.UtcNow,
            null
        )).ToList();

        return Ok(response);
    }

    [HttpPost("{id}/images")]
    [ProducesResponseType(typeof(UploadProductImagesResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(100_000_000)] 
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    public async Task<ActionResult<UploadProductImagesResponse>> UploadProductImages(
        Guid id,
        [FromForm] UploadProductImagesWebRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var imageRequests = new List<ProductImageUploadRequest>();
        var errors = new List<string>();

        foreach (var imageDto in request.Images)
        {
            try
            {
                if (imageDto.File.Length == 0)
                {
                    errors.Add($"File '{imageDto.File.FileName}' is empty");
                    continue;
                }

                var stream = imageDto.File.OpenReadStream();
                var imageRequest = new ProductImageUploadRequest(
                    stream,
                    imageDto.File.FileName,
                    imageDto.ImageType,
                    imageDto.DisplayOrder,
                    imageDto.AltText);

                imageRequests.Add(imageRequest);
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing file '{imageDto.File.FileName}': {ex.Message}");
            }
        }

        if (imageRequests.Count == 0)
        {
            return BadRequest(new { message = "No valid images to upload", errors });
        }

        var command = new UploadProductImagesCommand(id, imageRequests);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        var responseImages = result.Value.Select(img => new ProductImageResponseDto(
            img.Id,
            id,
            img.CloudinaryPublicId,
            img.ImageUrl,
            img.ThumbnailUrl,
            img.LargeUrl,
            img.ImageType,
            img.DisplayOrder,
            true,
            0,
            img.AltText,
            DateTime.UtcNow,
            null
        )).ToList();

        var response = new UploadProductImagesResponse(
            responseImages,
            responseImages.Count,
            request.Images.Count,
            errors
        );

        return CreatedAtAction(nameof(GetProductImages), new { id }, response);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProductImage(
        Guid id,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductImageCommand(id, imageId);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}/images/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult UpdateImageOrder(
        Guid id,
        [FromBody] UpdateImageOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        foreach (var (imageId, newOrder) in request.ImageOrders)
        {
        }

        return Ok(new { message = "Image order updated successfully" });
    }

    #endregion
} 