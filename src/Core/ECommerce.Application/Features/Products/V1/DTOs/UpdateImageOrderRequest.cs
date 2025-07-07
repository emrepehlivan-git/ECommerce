using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Products.V1.DTOs;

public sealed record UpdateImageOrderRequest(
    Dictionary<Guid, int> ImageOrders
);

public sealed record GetProductImagesQueryParams(
    ImageType? ImageType = null,
    bool ActiveOnly = true
); 