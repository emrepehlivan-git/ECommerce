using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.Features.Products.Queries;

public sealed class GetProductsByCategoryIdQueryValidator : AbstractValidator<GetProductsByCategoryIdQuery>
{
    public GetProductsByCategoryIdQueryValidator(ICategoryRepository categoryRepository, LocalizationHelper localizer)
    {
        RuleFor(x => x.CategoryId).MustAsync(async (categoryId, cancellationToken) =>
        {
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken: cancellationToken);
            return category is not null;
        }).WithMessage(localizer[ProductConsts.CategoryNotFound]);
    }
}
public sealed record GetProductsByCategoryIdQuery(Guid CategoryId) : IRequest<PagedResult<List<ProductDto>>>;

public sealed class GetProductsByCategoryIdQueryHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider)
: BaseHandler<GetProductsByCategoryIdQuery, PagedResult<List<ProductDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<ProductDto>>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
    {
        return await productRepository.GetPagedAsync<ProductDto>(
            predicate: p => p.CategoryId == request.CategoryId,
            include: p => p.Include(x => x.Category),
            cancellationToken: cancellationToken
        );
    }
}
