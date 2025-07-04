using System.Linq.Expressions;

namespace ECommerce.SharedKernel.Specifications;

public class AndSpecification<T> : BaseSpecification<T>
{
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        if (left.Criteria == null || right.Criteria == null)
            return;
            
        var paramExpr = Expression.Parameter(typeof(T), "x");
        var exprBody = Expression.AndAlso(
            ExpressionHelper.ReplaceParameter(left.Criteria.Body, left.Criteria.Parameters[0], paramExpr),
            ExpressionHelper.ReplaceParameter(right.Criteria.Body, right.Criteria.Parameters[0], paramExpr)
        );
        
        Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);
        
    
    }
} 