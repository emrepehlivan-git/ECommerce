using ECommerce.Application.Interfaces;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Services;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork, IScopedDependency
{
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var result = await action();
            await SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return result;
        }, cancellationToken);
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

    public int SaveChanges() => context.SaveChanges();
}
