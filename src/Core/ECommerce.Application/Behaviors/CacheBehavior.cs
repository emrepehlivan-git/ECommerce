using ECommerce.Application.Services;
using MediatR;

namespace ECommerce.Application.Behaviors;  

public sealed class CacheBehavior<TRequest, TResponse>(ICacheManager cacheManager) : IPipelineBehavior<TRequest, TResponse>
where TRequest : ICacheableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var cachedValue = await cacheManager.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedValue is not null)
            return cachedValue;

        var result = await next();

        await cacheManager.SetAsync(cacheKey, result, request.CacheDuration, cancellationToken);

        return result;
    }
}
