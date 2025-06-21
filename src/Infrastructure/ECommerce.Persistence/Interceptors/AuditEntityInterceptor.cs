using ECommerce.Application.Services;
using ECommerce.SharedKernel.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Persistence.Interceptors;

public sealed class AuditEntityInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Try to get the current user service from the current HTTP context
        var currentUserService = httpContextAccessor.HttpContext?.RequestServices.GetService<ICurrentUserService>();

        if (currentUserService is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ((IAuditableEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
                    ((IAuditableEntity)entry.Entity).CreatedBy = Guid.TryParse(currentUserService.UserId, out var createdBy) ? createdBy : null;
                    break;
                case EntityState.Modified:
                    ((IAuditableEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
                    ((IAuditableEntity)entry.Entity).UpdatedBy = Guid.TryParse(currentUserService.UserId, out var updatedBy) ? updatedBy : null;
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
