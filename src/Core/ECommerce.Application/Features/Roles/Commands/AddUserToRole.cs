using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.Commands;

public sealed record AddUserToRoleCommand(Guid UserId, string RoleName) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class AddUserToRoleCommandValidator : AbstractValidator<AddUserToRoleCommand>
{
    public AddUserToRoleCommandValidator(LocalizationHelper localizer, IUserService userService, IRoleService roleService)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (userId, cancellationToken) => await userService.FindByIdAsync(userId) != null)
            .WithMessage("UserId is required");

        RuleFor(x => x.RoleName)
            .MustAsync(async (roleName, cancellationToken) => await roleService.RoleExistsAsync(roleName))
            .WithMessage(localizer[RoleConsts.NameIsRequired]);
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

        var userRoles = await roleService.GetUserRolesAsync(user);
        if (userRoles.Contains(command.RoleName))
        {
            return Result.Error(Localizer[RoleConsts.UserAlreadyInRole]);
        }

        var result = await roleService.AddToRoleAsync(user, command.RoleName);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.Select(e => e.Description).ToArray());
        }

        await cacheManager.RemoveByPatternAsync($"user-roles:{command.UserId}:*", cancellationToken);

        return Result.Success();
    }
} 