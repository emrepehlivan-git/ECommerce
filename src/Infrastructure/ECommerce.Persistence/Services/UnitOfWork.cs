using ECommerce.Application.Interfaces;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Persistence.Services;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork, IScopedDependency
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() => await context.Database.BeginTransactionAsync();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

    public int SaveChanges() => context.SaveChanges();
}
