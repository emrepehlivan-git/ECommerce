using System.Linq.Expressions;

namespace ECommerce.SharedKernel.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>>? Includes { get; } = [];
    public List<string>? IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        => Includes?.Add(includeExpression);

    protected virtual void AddInclude(string includeString)
        => IncludeStrings?.Add(includeString);

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderBy = orderByExpression;

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        => OrderByDescending = orderByDescendingExpression;
}
