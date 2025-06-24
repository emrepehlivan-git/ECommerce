using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Services;
using ECommerce.Application.Helpers;
    using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Roles.Queries;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleDto>>, ICacheableRequest
{
    public string CacheKey => $"roles:id:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator(LocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(localizer[RoleConsts.RoleNotFound]);
    }
}

public sealed class GetRoleByIdQueryHandler(
    IRoleService roleService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetRoleByIdQuery, Result<RoleDto>>(lazyServiceProvider)
{
    public override async Task<Result<RoleDto>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await roleService.FindRoleByIdAsync(query.Id);
        
        if (role == null)
        {
            return Result.Error(Localizer[RoleConsts.RoleNotFound]);
        }
        
        var roleDto = role.Adapt<RoleDto>();
        
        return Result.Success(roleDto);
    }
} 