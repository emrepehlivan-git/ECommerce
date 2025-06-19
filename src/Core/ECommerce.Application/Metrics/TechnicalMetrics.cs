using System.Diagnostics.Metrics;

namespace ECommerce.Application.Metrics;

public sealed class TechnicalMetrics
{
    private static readonly Meter Meter = new("ECommerce.Technical");

    // Database Metrics
    private static readonly Histogram<double> DatabaseQueryDuration = 
        Meter.CreateHistogram<double>("database_query_duration", "seconds", "Database query execution time");

    // Cache Metrics
    private static readonly Counter<long> CacheHitsCounter = 
        Meter.CreateCounter<long>("cache_hits_total", "Total cache hits");
    
    private static readonly Counter<long> CacheMissesCounter = 
        Meter.CreateCounter<long>("cache_misses_total", "Total cache misses");

    // API Metrics
    private static readonly Counter<long> ApiRequestsCounter = 
        Meter.CreateCounter<long>("api_requests_total", "Total API requests");

    public static void RecordDatabaseQuery(double durationSeconds, string operation, string table)
    {
        DatabaseQueryDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("table", table));
    }

    public static void RecordCacheHit(string cacheKey)
    {
        CacheHitsCounter.Add(1,
            new KeyValuePair<string, object?>("cache_key", cacheKey));
    }

    public static void RecordCacheMiss(string cacheKey)
    {
        CacheMissesCounter.Add(1,
            new KeyValuePair<string, object?>("cache_key", cacheKey));
    }

    public static void RecordApiRequest(string endpoint, string method, int statusCode)
    {
        ApiRequestsCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("status_code", statusCode));
    }
} 