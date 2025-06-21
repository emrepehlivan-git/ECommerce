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
    Task<IList<Role>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string role);
} 