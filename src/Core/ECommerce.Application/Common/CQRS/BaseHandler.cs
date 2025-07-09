using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Common.CQRS;

public abstract class BaseHandler<TRequest, TResponse>(ILazyServiceProvider lazyServiceProvider) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected ILazyServiceProvider LazyServiceProvider { get; } = lazyServiceProvider;

    protected ILocalizationHelper Localizer => LazyServiceProvider.LazyGetRequiredService<ILocalizationHelper>();

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

}
