using System.Linq.Expressions;

namespace ECommerce.SharedKernel.Specifications;

public static class ExpressionHelper
{
    public static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
    {
        return new ParameterReplacer(source, target).Visit(expression);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ParameterReplacer(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }
} 