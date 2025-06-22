using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.Commands;

public sealed record UpdateRoleCommand(Guid Id, string Name) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly IRoleService _roleService;
    private readonly LocalizationHelper _localizer;

    public UpdateRoleCommandValidator(IRoleService roleService, LocalizationHelper localizer)
    {
        _roleService = roleService;
        _localizer = localizer;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) => await roleService.FindRoleByIdAsync(id) != null)
            .WithMessage(localizer[RoleConsts.RoleNotFound]);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_localizer[RoleConsts.NameIsRequired])
            .MinimumLength(RoleConsts.NameMinLength)
            .WithMessage(_localizer[RoleConsts.NameMustBeAtLeastCharacters])
            .MaximumLength(RoleConsts.NameMaxLength)
            .WithMessage(_localizer[RoleConsts.NameMustBeLessThanCharacters]);
            
        RuleFor(x => x)
            .MustAsync(async (command, ct) => 
            {
                var existingRole = await _roleService.FindRoleByNameAsync(command.Name);
                return existingRole == null || existingRole.Id == command.Id;
            })
            .WithMessage(_localizer[RoleConsts.NameExists]);
    }
}

public sealed class UpdateRoleCommandHandler(
    IRoleService roleService,
    ICacheManager cacheManager,
    ILazyServiceProvider lazyServiceProvider) :
    BaseHandler<UpdateRoleCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = (await roleService.FindRoleByIdAsync(command.Id))!;
        
        role.UpdateName(command.Name);

        var result = await roleService.UpdateRoleAsync(role);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.Select(e => e.Description).ToArray());
        }

        await cacheManager.RemoveByPatternAsync("roles:*", cancellationToken);

        return Result.Success();
    }
} 