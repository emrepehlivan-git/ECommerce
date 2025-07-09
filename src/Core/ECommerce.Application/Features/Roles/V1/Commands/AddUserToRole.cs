using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Users;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record AddUserToRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class AddUserToRoleCommandValidator : AbstractValidator<AddUserToRoleCommand>
{
    public AddUserToRoleCommandValidator(LocalizationHelper localizer, IUserService userService, IRoleService roleService)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (userId, cancellationToken) => await userService.FindByIdAsync(userId) != null)
            .WithMessage(localizer[UserConsts.NotFound]);

        RuleFor(x => x.RoleId)
            .MustAsync(async (roleId, cancellationToken) => await roleService.FindRoleByIdAsync(roleId) != null)
            .WithMessage(localizer[RoleConsts.RoleNotFound]);
    }
}

public sealed class AddUserToRoleCommandHandler(
    IRoleService roleService,
    IUserService userService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<AddUserToRoleCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(AddUserToRoleCommand command, CancellationToken cancellationToken)
    {
        var user = (await userService.FindByIdAsync(command.UserId))!;
        var role = (await roleService.FindRoleByIdAsync(command.RoleId))!;

        var userRoles = await roleService.GetUserRolesAsync(user);
        if (userRoles.Contains(role.Name!))
        {
            return Result.Error(Localizer[RoleConsts.UserAlreadyInRole]);
        }

        var result = await roleService.AddToRoleAsync(user, role.Name!);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.Select(e => e.Description).ToArray());
        }

        await cacheManager.RemoveByPatternAsync($"user-roles:{command.UserId}:*", cancellationToken);

        return Result.Success();
    }
} 