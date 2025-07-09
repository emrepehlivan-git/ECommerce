using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record CreateRoleCommand(string Name) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{

    public CreateRoleCommandValidator(IRoleService roleService, ILocalizationHelper localizer)
    {

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(x => localizer[RoleConsts.NameIsRequired])
            .MinimumLength(RoleConsts.NameMinLength)
                .WithMessage(x => localizer[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()])
            .MaximumLength(RoleConsts.NameMaxLength)
                .WithMessage(x => localizer[RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()])
            .MustAsync(async (name, cancellationToken) => !await roleService.RoleExistsAsync(name))
                .WithMessage(x => localizer[RoleConsts.NameExists]);
    }
}

public sealed class CreateRoleCommandHandler(
    IRoleService roleService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<CreateRoleCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = Role.Create(command.Name);

        var result = await roleService.CreateRoleAsync(role);

        if (!result.Succeeded)
        {
            return Result.Error([.. result.Errors.Select(e => e.Description)]);
        }

        await cacheManager.RemoveByPatternAsync("roles:all:include-permissions:*", cancellationToken);

        return Result.Success(role.Id);
    }
} 