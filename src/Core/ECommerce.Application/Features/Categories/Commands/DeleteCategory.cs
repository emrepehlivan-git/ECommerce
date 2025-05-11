using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>, ITransactionalRequest;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<DeleteCategoryCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (category is null)
            return Result.NotFound(Localizer[CategoryConsts.NotFound]);

        if (category.Products.Any())
            return Result.Conflict(Localizer[CategoryConsts.CannotDeleteWithProducts]);

        categoryRepository.Delete(category);

        return Result.Success();
    }
}