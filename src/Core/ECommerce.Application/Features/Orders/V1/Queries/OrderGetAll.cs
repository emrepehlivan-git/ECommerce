using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.V1.Queries;

public sealed record OrderGetAllQuery(
    PageableRequestParams PageableRequestParams,
    OrderStatus? Status = null) : IRequest<PagedResult<List<OrderDto>>>;

public sealed class OrderGetAllQueryHandler(
    IOrderRepository orderRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderGetAllQuery, PagedResult<List<OrderDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<OrderDto>>> Handle(OrderGetAllQuery query, CancellationToken cancellationToken)
    {
        return await orderRepository.GetPagedAsync<OrderDto>(
            predicate: query.Status.HasValue ? x => x.Status == query.Status.Value : null,
            orderBy: q => q.OrderByDescending(x => x.OrderDate),
            include: q => q.Include(x => x.Items).ThenInclude(x => x.Product),
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}