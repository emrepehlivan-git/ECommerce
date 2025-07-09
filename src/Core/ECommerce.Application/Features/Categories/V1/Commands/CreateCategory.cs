using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Categories.V1.Commands;

public sealed record CreateCategoryCommand(string Name) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(CategoryBusinessRules categoryBusinessRules, LocalizationHelper localizer)
    {

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizer[CategoryConsts.NameIsRequired])
            .MinimumLength(CategoryConsts.NameMinLength)
            .WithMessage(localizer[CategoryConsts.NameMustBeAtLeastCharacters])
            .MaximumLength(CategoryConsts.NameMaxLength)
            .WithMessage(localizer[CategoryConsts.NameMustBeLessThanCharacters])
            .MustAsync(async (name, ct) =>
                !await categoryBusinessRules.CheckIfCategoryExistsAsync(name, cancellationToken: ct))
            .WithMessage(localizer[CategoryConsts.NameExists]);
    }
}

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<CreateCategoryCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = Category.Create(command.Name);

        categoryRepository.Add(category);

        await cacheManager.RemoveByPatternAsync("categories:*", cancellationToken);

        return Result.Success(category.Id);
    }
}