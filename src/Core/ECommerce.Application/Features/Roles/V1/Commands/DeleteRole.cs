using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record DeleteRoleCommand(Guid Id) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator(IRoleService roleService, ILocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) => await roleService.FindRoleByIdAsync(id) != null)
            .WithMessage(localizer[RoleConsts.RoleNotFound]);
    }
}

public sealed class DeleteRoleCommandHandler(
    IRoleService roleService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<DeleteRoleCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = (await roleService.FindRoleByIdAsync(command.Id))!;
        
        var result = await roleService.DeleteRoleAsync(role);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.Select(e => e.Description).ToArray());
        }

        await cacheManager.RemoveByPatternAsync("roles:*", cancellationToken);

        return Result.Success();
    }
} 