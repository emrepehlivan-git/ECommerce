using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ECommerce.Application.Instrumentation;

public static class ApplicationInstrumentation
{
    private static readonly ActivitySource ActivitySource = new("ECommerce.Application");
    private static readonly Meter Meter = new("ECommerce.Application");

    // Counters
    private static readonly Counter<long> OrdersCreatedCounter = 
        Meter.CreateCounter<long>("orders_created_total", "Total number of orders created");
    
    private static readonly Counter<long> PaymentProcessedCounter = 
        Meter.CreateCounter<long>("payments_processed_total", "Total number of payments processed");

    // Histograms
    private static readonly Histogram<double> OrderProcessingDuration = 
        Meter.CreateHistogram<double>("order_processing_duration", "seconds", "Time taken to process an order");

    // Gauges
    private static readonly ObservableGauge<int> ActiveUsersGauge = 
        Meter.CreateObservableGauge<int>("active_users", () => GetActiveUsersCount(), "Number of active users");

    private static int GetActiveUsersCount()
    {
        // This would be implemented to return actual active user count
        // For now, return a placeholder value
        return 0;
    }

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static void RecordOrderCreated(string orderStatus, decimal amount)
    {
        OrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>("status", orderStatus));
        
        using var activity = StartActivity("order.created");
        activity?.SetTag("order.status", orderStatus);
        activity?.SetTag("order.amount", amount);
    }

    public static void RecordOrderProcessingTime(double durationSeconds, string orderType)
    {
        OrderProcessingDuration.Record(durationSeconds, 
            new KeyValuePair<string, object?>("order.type", orderType));
    }

    public static void RecordPaymentProcessed(string paymentMethod, decimal amount, string status)
    {
        PaymentProcessedCounter.Add(1, 
            new KeyValuePair<string, object?>("payment_method", paymentMethod),
            new KeyValuePair<string, object?>("status", status));
        
        using var activity = StartActivity("payment.processed");
        activity?.SetTag("payment.method", paymentMethod);
        activity?.SetTag("payment.amount", amount);
        activity?.SetTag("payment.status", status);
    }
} 