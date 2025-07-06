using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public interface IUserSynchronizationService
{
    Task<Result<User>> SyncUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
} 