using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Users;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Result<bool>>;

public sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator(LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(localizer[RoleConsts.UserNotFound]);

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage(localizer[RoleConsts.RoleNotFound]);
    }
}

public sealed class AssignRoleToUserCommandHandler(
    IRoleService roleService,
    IUserService userService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<AssignRoleToUserCommand, Result<bool>>(lazyServiceProvider)
{
    public override async Task<Result<bool>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<bool>.Error(Localizer[RoleConsts.UserNotFound]);
        }

        var role = await roleService.FindRoleByIdAsync(request.RoleId);
        if (role == null)
        {
            return Result<bool>.Error(Localizer[RoleConsts.RoleNotFound]);
        }

        var result = await roleService.AddToRoleAsync(user, role.Name!);
        
        // User permissions cache'ini temizle
        await cacheManager.RemoveAsync($"user_permissions_{request.UserId}", cancellationToken);

        return Result<bool>.Success(result.Succeeded);
    }
} 