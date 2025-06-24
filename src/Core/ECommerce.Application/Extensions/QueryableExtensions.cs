using System.Linq.Expressions;
using System.Reflection;
using Ardalis.Result;
using ECommerce.Application.Parameters;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ECommerce.Application.Extensions;

public static class QueryableExtensions
{
    public static PagedResult<List<TResult>> ApplyPaging<T, TResult>(this IQueryable<T> query, PageableRequestParams pageableRequestParams)
    {
        ArgumentNullException.ThrowIfNull(query);
        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageableRequestParams.PageSize);
        var pageInfo = new PagedInfo(pageableRequestParams.Page, pageableRequestParams.PageSize, totalPages, totalCount);
        var items = query.Take(((pageableRequestParams.Page - 1) * pageableRequestParams.PageSize)..pageableRequestParams.PageSize).ToList();
        return new PagedResult<List<TResult>>(pageInfo, items.Adapt<List<TResult>>());
    }

    public static async Task<PagedResult<List<TDestination>>> ApplyPagingAsync<TSource, TDestination>(
    this IQueryable<TSource> query,
    PageableRequestParams pageableRequestParams,
    Expression<Func<TSource, bool>>? predicate = null,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        int count;
        List<TDestination> items;

        var skip = (pageableRequestParams.Page - 1) * pageableRequestParams.PageSize;
        var take = pageableRequestParams.PageSize;

        if (query.Provider is IAsyncQueryProvider)
        {
            count = await query.CountAsync(predicate ?? (x => true), cancellationToken);

            if (typeof(TSource) == typeof(TDestination))
            {
                var list = await query
                    .Skip(skip)
                    .Take(take)
                    .Cast<TDestination>()
                    .ToListAsync(cancellationToken);

                items = list;
            }
            else
            {
                items = await query
                    .Skip(skip)
                    .Take(take)
                    .ProjectToType<TDestination>()
                    .ToListAsync(cancellationToken);
            }
        }
        else
        {
            count = query.Count(predicate ?? (x => true));

            var sourceItems = query
                .Skip(skip)
                .Take(take)
                .ToList();

            if (typeof(TSource) == typeof(TDestination))
            {
                items = sourceItems.Cast<TDestination>().ToList();
            }
            else
            {
                items = sourceItems.Adapt<List<TDestination>>();
            }
        }

        var totalPages = (int)Math.Ceiling((double)count / pageableRequestParams.PageSize);
        var pageInfo = new PagedInfo(pageableRequestParams.Page, pageableRequestParams.PageSize, totalPages, count);
        return new PagedResult<List<TDestination>>(pageInfo, items);
    }


    public static IQueryable<T> IncludeIf<T>(this IQueryable<T> query,
    bool condition,
    Expression<Func<T, object>> include)
    where T : class
    {
        return condition ? query.Include(include) : query;
    }

    public static IOrderedQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, Filter filter)
    {
        if (filter.OrderByFields.Count == 0)
            return query.OrderBy(x => 1);

        IOrderedQueryable<T>? orderedQuery = null;

        for (int i = 0; i < filter.OrderByFields.Count; i++)
        {
            var orderByField = filter.OrderByFields[i];
            
            if (i == 0)
            {
                orderedQuery = orderByField.IsDescending
                    ? ApplyOrder(query, orderByField.PropertyName, "OrderByDescending")
                    : ApplyOrder(query, orderByField.PropertyName, "OrderBy");
            }
            else
            {
                orderedQuery = orderByField.IsDescending
                    ? ApplyOrder(orderedQuery!, orderByField.PropertyName, "ThenByDescending")
                    : ApplyOrder(orderedQuery!, orderByField.PropertyName, "ThenBy");
            }
        }

        return (IOrderedQueryable<T>)(orderedQuery ?? query);
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
    {
        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        
        foreach (string prop in props)
        {
            PropertyInfo? pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentException($"Property '{prop}' not found on type '{type.Name}'");
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
        }
        
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

        object? result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, [source, lambda]);

        return (IOrderedQueryable<T>)(result ?? throw new InvalidOperationException($"Failed to apply order method '{methodName}'"));
    }
}