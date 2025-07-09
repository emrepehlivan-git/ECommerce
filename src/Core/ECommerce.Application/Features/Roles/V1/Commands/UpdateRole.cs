using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Commands;

public sealed record UpdateRoleCommand(Guid Id, string Name) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly ILocalizationHelper _localizer;
    private readonly IRoleService _roleService;

    public UpdateRoleCommandValidator(ILocalizationHelper localizer, IRoleService roleService)
    {
        _localizer = localizer;
        _roleService = roleService;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) => await roleService.FindRoleByIdAsync(id) != null)
            .WithMessage(x => _localizer[RoleConsts.RoleNotFound]);

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(x => _localizer[RoleConsts.NameIsRequired])
            .MinimumLength(RoleConsts.NameMinLength)
                .WithMessage(x => _localizer[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()])
            .MaximumLength(RoleConsts.NameMaxLength)
                .WithMessage(x => _localizer[RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()]);
            
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var existingRole = await roleService.FindRoleByNameAsync(command.Name);
                return existingRole == null || existingRole.Id == command.Id;
            })
            .WithMessage(x => _localizer[RoleConsts.NameExists])
            .WithName(nameof(UpdateRoleCommand.Name));
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

        await cacheManager.RemoveByPatternAsync("roles:all:include-permissions:*", cancellationToken);

        return Result.Success();
    }
} 