using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Categories.V1.Commands;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(CategoryBusinessRules categoryBusinessRules, ILocalizationHelper localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizer[CategoryConsts.NameIsRequired])
            .MinimumLength(CategoryConsts.NameMinLength)
            .WithMessage(localizer[CategoryConsts.NameMustBeAtLeastCharacters])
            .MaximumLength(CategoryConsts.NameMaxLength)
            .WithMessage(localizer[CategoryConsts.NameMustBeLessThanCharacters])
            .MustAsync(async (command, name, ct) =>
                !await categoryBusinessRules.CheckIfCategoryExistsAsync(name, command.Id, ct))
            .WithMessage(localizer[CategoryConsts.NameExists]);
    }
}

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateCategoryCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (category is null)
            return Result.NotFound(Localizer[CategoryConsts.NotFound]);
        
        category.UpdateName(command.Name);

        categoryRepository.Update(category);

        await cacheManager.RemoveAsync($"category:{command.Id}", cancellationToken);
        await cacheManager.RemoveByPatternAsync("categories:*", cancellationToken);
        await cacheManager.RemoveByPatternAsync("products:*", cancellationToken);

        return Result.Success();
    }
}