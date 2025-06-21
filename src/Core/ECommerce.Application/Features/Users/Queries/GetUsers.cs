using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Application.Services;
using ECommerce.Application.Parameters;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Users.Queries;

public sealed record GetUsersQuery(PageableRequestParams PageableRequestParams) : IRequest<PagedResult<List<UserDto>>>;

public sealed class GetUsersQueryHandler(
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUsersQuery, PagedResult<List<UserDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<UserDto>>> Handle(GetUsersQuery query,
    CancellationToken cancellationToken)
    {
        return await userService.Users
            .AsNoTracking()
            .ApplyPagingAsync<User, UserDto>(query.PageableRequestParams, cancellationToken: cancellationToken);
    }
}
