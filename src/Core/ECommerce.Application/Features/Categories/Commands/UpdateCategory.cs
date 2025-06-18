using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(CategoryBusinessRules categoryBusinessRules, ICategoryRepository categoryRepository, LocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (command, id, ct) => await categoryRepository.GetByIdAsync(id, cancellationToken: ct) is not null)
            .WithMessage(localizer[CategoryConsts.NotFound]);

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
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateCategoryCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        category!.UpdateName(command.Name);

        categoryRepository.Update(category);

        return Result.Success();
    }
}