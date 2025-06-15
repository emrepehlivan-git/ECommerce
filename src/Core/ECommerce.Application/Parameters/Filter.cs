namespace ECommerce.Application.Parameters;

public sealed record OrderByField(string PropertyName, bool IsDescending = false);

public sealed record Filter
{
    public List<OrderByField> OrderByFields { get; init; } = [];
    
    public static Filter Create() => new();
    
    public Filter OrderBy(string propertyName, bool isDescending = false)
    {
        var newOrderByFields = OrderByFields.ToList();
        newOrderByFields.Add(new OrderByField(propertyName, isDescending));
        return this with { OrderByFields = newOrderByFields };
    }
    
    public Filter OrderByDescending(string propertyName)
    {
        return OrderBy(propertyName, true);
    }
    
    public static Filter FromOrderByString(string? orderBy)
    {
        var filter = Create();
        
        if (string.IsNullOrWhiteSpace(orderBy))
            return filter;
            
        var orderByFields = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var field in orderByFields)
        {
            var trimmedField = field.Trim();
            var isDescending = trimmedField.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
            
            var propertyName = isDescending 
                ? trimmedField[..^5].Trim() 
                : trimmedField.EndsWith(" asc", StringComparison.OrdinalIgnoreCase)
                    ? trimmedField[..^4].Trim()
                    : trimmedField;
                    
            filter = filter.OrderBy(propertyName, isDescending);
        }
        
        return filter;
    }
} 