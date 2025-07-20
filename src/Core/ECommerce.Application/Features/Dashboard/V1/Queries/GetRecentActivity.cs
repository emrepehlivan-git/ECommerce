using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Dashboard.V1.Queries;

public sealed record GetRecentActivityQuery(int Count = 10) : IRequest<List<RecentActivityResult>>;

public sealed class GetRecentActivityQueryHandler(
    IUserService userService,
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetRecentActivityQuery, List<RecentActivityResult>>(lazyServiceProvider)
{
    public override async Task<List<RecentActivityResult>> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
    {
        var activities = new List<RecentActivityResult>();

        // Get recent orders
        var recentOrders = await orderRepository.Query(
            orderBy: q => q.OrderByDescending(o => o.OrderDate)
        ).Take(3).ToListAsync(cancellationToken);

        activities.AddRange(recentOrders.Select(order => new RecentActivityResult
        {
            Type = "order",
            Title = "New order received",
            Description = $"Order #{order.Id} placed",
            Icon = "ShoppingCart",
            Color = "text-blue-600",
            Timestamp = order.OrderDate
        }));

        // Get recent users
        var recentUsers = await userService.Users.Take(3).ToListAsync(cancellationToken);
        activities.AddRange(recentUsers.Select(user => new RecentActivityResult
        {
            Type = "user",
            Title = "New user registered",
            Description = $"{user.Email} joined",
            Icon = "Users",
            Color = "text-green-600",
            Timestamp = DateTime.UtcNow.AddMinutes(-30)
        }));

        // Get low stock products
        var lowStockProducts = await productRepository.Query(
            predicate: p => p.Stock != null && p.Stock.Quantity < 10,
            include: q => q.Include(p => p.Stock)
        ).Take(3).ToListAsync(cancellationToken);

        activities.AddRange(lowStockProducts.Select(product => new RecentActivityResult
        {
            Type = "stock",
            Title = "Low stock alert",
            Description = $"Product \"{product.Name}\" has low stock ({product.Stock!.Quantity} remaining)",
            Icon = "AlertTriangle",
            Color = "text-orange-600",
            Timestamp = DateTime.UtcNow.AddHours(-1)
        }));

        // Sort by timestamp and take the requested count
        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Count)
            .ToList();
    }
}

public class RecentActivityResult
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}