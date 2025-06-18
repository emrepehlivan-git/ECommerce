using System.Linq.Expressions;

namespace ECommerce.SharedKernel.Specifications;

public class NotSpecification<T> : BaseSpecification<T>
{
    public NotSpecification(ISpecification<T> specification)
    {
        if (specification.Criteria == null)
            return;
            
        var paramExpr = Expression.Parameter(typeof(T), "x");
        var exprBody = Expression.Not(
            ExpressionHelper.ReplaceParameter(specification.Criteria.Body, specification.Criteria.Parameters[0], paramExpr)
        );
        
        Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);
    }
} 