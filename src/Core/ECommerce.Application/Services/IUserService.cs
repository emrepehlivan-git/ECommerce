using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services;

public interface IUserService
{
    IQueryable<User> Users { get; }
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(Guid userId);
    Task<User?> GetUserByPrincipalAsync(ClaimsPrincipal principal);
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<IdentityResult> UpdateAsync(User user);
    Task<bool> CanSignInAsync(User user);
    Task<Result> UpdateBirthdayAsync(Guid userId, DateTime? birthday);
} 