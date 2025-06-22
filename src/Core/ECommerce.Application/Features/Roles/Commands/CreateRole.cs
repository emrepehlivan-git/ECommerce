using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.Commands;

public sealed record CreateRoleCommand(string Name) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly IRoleService _roleService;
    private readonly LocalizationHelper _localizer;

    public CreateRoleCommandValidator(IRoleService roleService, LocalizationHelper localizer)
    {
        _roleService = roleService;
        _localizer = localizer;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_localizer[RoleConsts.NameIsRequired])
            .MinimumLength(RoleConsts.NameMinLength)
            .WithMessage(_localizer[RoleConsts.NameMustBeAtLeastCharacters])
            .MaximumLength(RoleConsts.NameMaxLength)
            .WithMessage(_localizer[RoleConsts.NameMustBeLessThanCharacters])
            .MustAsync(async (name, ct) => !await _roleService.RoleExistsAsync(name))
            .WithMessage(_localizer[RoleConsts.NameExists]);
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

        await cacheManager.RemoveByPatternAsync("roles:*", cancellationToken);

        return Result.Success(role.Id);
    }
} 