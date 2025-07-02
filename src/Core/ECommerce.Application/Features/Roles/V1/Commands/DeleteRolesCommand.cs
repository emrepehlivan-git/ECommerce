using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record DeleteRolesCommand(List<Guid> Ids) : IRequest<Result>, ITransactionalRequest;

public sealed class DeleteRolesCommandHandler(
    IRoleService roleService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<DeleteRolesCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteRolesCommand command, CancellationToken cancellationToken)
    {
        var roles = await roleService.FindRolesByIdsAsync(command.Ids, cancellationToken);
        if (roles.Count != command.Ids?.Count)
            return Result.Error(Localizer[RoleConsts.RoleNotFound]);

        var result = await roleService.DeleteRolesAsync(roles);
        if (!result.Succeeded)
            return Result.Error([.. result.Errors.Select(e => e.Description)]);

        await cacheManager.RemoveAsync("roles:all:include-permissions:True", cancellationToken);
        await cacheManager.RemoveAsync("roles:all:include-permissions:False", cancellationToken);

        return Result.Success();
    }
} 