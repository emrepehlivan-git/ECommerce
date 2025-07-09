using ECommerce.Application.Common.Logging;
using ECommerce.Application.Interfaces;
using MediatR;
namespace ECommerce.Application.Behaviors;

public sealed class TransactionalRequestBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IECommerceLogger<TransactionalRequestBehavior<TRequest, TResponse>> logger)
 : IPipelineBehavior<TRequest, TResponse>
 where TRequest : ITransactionalRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () => await next(), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Transaction failed for request {typeof(TRequest).Name}");
            throw;
        }
    }
}
