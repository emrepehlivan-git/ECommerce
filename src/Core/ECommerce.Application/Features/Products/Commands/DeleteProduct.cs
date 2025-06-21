using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>, ITransactionalRequest;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<DeleteProductCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        productRepository.Delete(product);

        // Cache invalidation
        await cacheManager.RemoveAsync($"product:{command.Id}", cancellationToken);
        await cacheManager.RemoveByPatternAsync("products:*", cancellationToken);

        return Result.Success();
    }
}