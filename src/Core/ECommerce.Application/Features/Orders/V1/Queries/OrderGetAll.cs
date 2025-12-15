using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.V1.Queries;

public sealed record OrderGetAllQuery(
    PageableRequestParams PageableRequestParams,
    OrderStatus? Status = null) : IRequest<PagedResult<List<OrderDto>>>;

public sealed class OrderGetAllQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderGetAllQuery, PagedResult<List<OrderDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<OrderDto>>> Handle(OrderGetAllQuery query, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUserService.UserId!);

        return await orderRepository.GetPagedAsync<OrderDto>(
            predicate: x => x.UserId == userId && (!query.Status.HasValue || x.Status == query.Status.Value),
            orderBy: q => q.OrderByDescending(x => x.OrderDate),
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}