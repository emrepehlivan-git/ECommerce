using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Users.V1.DTOs;
using ECommerce.Application.Features.Users.V1.Specifications;
using ECommerce.Application.Services;
using ECommerce.Application.Parameters;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Users.V1.Queries;

public sealed record GetUsersQuery(PageableRequestParams PageableRequestParams) : IRequest<PagedResult<List<UserDto>>>;

public sealed class GetUsersQueryHandler(
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUsersQuery, PagedResult<List<UserDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<UserDto>>> Handle(GetUsersQuery query,
    CancellationToken cancellationToken)
    {
        var spec = new UserSearchSpecification(query.PageableRequestParams.Search);
        var usersQuery = SpecificationEvaluator<User>.GetQuery(userService.Users.AsNoTracking(), spec);
        return await usersQuery.ApplyPagingAsync<User, UserDto>(query.PageableRequestParams, cancellationToken: cancellationToken);
    }
}
