using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>, ITransactionalRequest;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<DeleteCategoryCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (category is null)
            return Result.NotFound(Localizer[CategoryConsts.NotFound]);

        if (await categoryRepository.HasProductsAsync(command.Id, cancellationToken))
            return Result.Conflict(Localizer[CategoryConsts.CannotDeleteWithProducts]);

        categoryRepository.Delete(category);

        // Cache invalidation
        await cacheManager.RemoveAsync($"category:{command.Id}", cancellationToken);
        await cacheManager.RemoveByPatternAsync("categories:*", cancellationToken);
        // Also clear products cache as they might include category info
        await cacheManager.RemoveByPatternAsync("products:*", cancellationToken);

        return Result.Success();
    }
}