using System.Diagnostics.Metrics;

namespace ECommerce.Application.Metrics;

public sealed class BusinessMetrics
{
    private static readonly Meter Meter = new("ECommerce.Business");

    // Order Metrics
    private static readonly Counter<long> OrdersCounter = 
        Meter.CreateCounter<long>("orders_total", "Total number of orders");
    
    private static readonly Histogram<double> OrderValueHistogram = 
        Meter.CreateHistogram<double>("order_value", "currency", "Order value distribution");

    // Product Metrics
    private static readonly Counter<long> ProductViewsCounter = 
        Meter.CreateCounter<long>("product_views_total", "Total product views");

    // User Metrics
    private static readonly Counter<long> UserRegistrationsCounter = 
        Meter.CreateCounter<long>("user_registrations_total", "Total user registrations");

    public static void RecordOrder(decimal value, string status, string paymentMethod)
    {
        OrdersCounter.Add(1, 
            new KeyValuePair<string, object?>("status", status),
            new KeyValuePair<string, object?>("payment_method", paymentMethod));
        
        OrderValueHistogram.Record((double)value,
            new KeyValuePair<string, object?>("status", status));
    }

    public static void RecordProductView(string categoryName, string productId)
    {
        ProductViewsCounter.Add(1,
            new KeyValuePair<string, object?>("category", categoryName),
            new KeyValuePair<string, object?>("product_id", productId));
    }

    public static void RecordUserRegistration(string registrationType)
    {
        UserRegistrationsCounter.Add(1,
            new KeyValuePair<string, object?>("registration_type", registrationType));
    }
} 