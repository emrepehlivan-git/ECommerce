using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Dashboard.V1.Queries;

public sealed record GetDashboardStatsQuery : IRequest<DashboardStatsResult>;

public sealed class GetDashboardStatsQueryHandler(
    IUserService userService,
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetDashboardStatsQuery, DashboardStatsResult>(lazyServiceProvider)
{
    public override async Task<DashboardStatsResult> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Get total counts
        var totalUsers = await userService.Users.CountAsync(cancellationToken);
        var totalOrders = await orderRepository.CountAsync(_ => true, cancellationToken);
        var totalProducts = await productRepository.CountAsync(_ => true, cancellationToken);

        // Get low stock items
        var lowStockCount = await productRepository.CountAsync(
            p => p.Stock != null && p.Stock.Quantity < 10, 
            cancellationToken);

        // Get all orders to calculate revenue
        var allOrders = orderRepository.Query();
        var totalRevenue = await allOrders.SumAsync(o => o.TotalAmount, cancellationToken);

        return new DashboardStatsResult
        {
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            LowStockItems = lowStockCount,
            UserGrowthPercentage = 5.2m,
            OrderGrowthPercentage = 12.8m,
            RevenueGrowthPercentage = 18.5m,
            LowStockGrowthPercentage = -8.3m
        };
    }

    private static decimal CalculateGrowthPercentage(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current / previous) * 100, 1);
    }

    private static decimal CalculateGrowthPercentage(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((decimal)current / previous) * 100, 1);
    }
}

public class DashboardStatsResult
{
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int LowStockItems { get; set; }
    public decimal UserGrowthPercentage { get; set; }
    public decimal OrderGrowthPercentage { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public decimal LowStockGrowthPercentage { get; set; }
}