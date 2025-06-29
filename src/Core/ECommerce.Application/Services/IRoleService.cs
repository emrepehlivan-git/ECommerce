using Ardalis.Result;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services;

public interface IRoleService
{
    Task<IList<string>> GetRolesAsync();
    Task<IList<string>> GetUserRolesAsync(User user);
    Task<Role?> FindRoleByIdAsync(Guid roleId);
    Task<Role?> FindRoleByNameAsync(string roleName);
    Task<IdentityResult> CreateRoleAsync(Role role);
    Task<IdentityResult> UpdateRoleAsync(Role role);
    Task<IdentityResult> DeleteRoleAsync(Role role);
    Task<PagedResult<List<RoleDto>>> GetAllRolesAsync(int page, int pageSize, string search, bool includePermissions = false);
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string role);
} 