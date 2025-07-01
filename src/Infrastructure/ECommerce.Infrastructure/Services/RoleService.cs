using Ardalis.Result;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public sealed class RoleService(UserManager<User> userManager, RoleManager<Role> roleManager) : IRoleService, IScopedDependency
{
    public async Task<IList<string>> GetRolesAsync()
    {
        return await roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<Role?> FindRoleByIdAsync(Guid roleId)
    {
        return await roleManager.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<List<Role>> FindRolesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        return await roleManager.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(cancellationToken);
    }

    public async Task<Role?> FindRoleByNameAsync(string roleName)
    {
        return await roleManager.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<IdentityResult> CreateRoleAsync(Role role)
    {
        return await roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateRoleAsync(Role role)
    {
        return await roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(Role role)
    {
        return await roleManager.DeleteAsync(role);
    }

    public async Task<IdentityResult> DeleteRolesAsync(List<Role> roles)
    {
        foreach (var role in roles)
        {
            var result = await DeleteRoleAsync(role);
            if (!result.Succeeded)
                return result;
        }
        return IdentityResult.Success;
    }

    public async Task<PagedResult<List<RoleDto>>> GetAllRolesAsync(int page, int pageSize, string search, bool includePermissions = false)
    {
        IQueryable<Role> query = roleManager.Roles;

        if (includePermissions)
        {
            query = query.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission);
        }

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Name.ToLower().Contains(search.ToLower()));

        return await query.ApplyPagingAsync<Role, RoleDto>(new PageableRequestParams(page, pageSize), cancellationToken: CancellationToken.None);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        return await userManager.RemoveFromRoleAsync(user, role);
    }
} 