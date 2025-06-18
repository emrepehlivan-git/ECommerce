using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands;

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
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<CreateCategoryCommand, Result<Guid>>(lazyServiceProvider)
{
    public override Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = Category.Create(command.Name);

        categoryRepository.Add(category);

        return Task.FromResult(Result.Success(category.Id));
    }
}