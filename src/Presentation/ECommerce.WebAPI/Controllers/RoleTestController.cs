using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Services;
using ECommerce.Application.Extensions;
using System.Security.Claims;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleTestController : ControllerBase
{
    private readonly IUserService UserService;
    private readonly IRoleService RoleService;
    private readonly IKeycloakRoleSyncService KeycloakRoleSyncService;

    public RoleTestController(
        IUserService userService,
        IRoleService roleService,
        IKeycloakRoleSyncService keycloakRoleSyncService)
    {
        UserService = userService;
        RoleService = roleService;
        KeycloakRoleSyncService = keycloakRoleSyncService;
    }

    [HttpGet("client-roles")]
    public async Task<IActionResult> GetClientRoleInfo()
    {
        var user = await UserService.GetUserByPrincipalAsync(User);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        var userRoles = await RoleService.GetUserRolesAsync(user);
        var clientRoles = User.GetClientRoles();
        
        var tokenInfo = new
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName.ToString(),
            
            ClientRoles = clientRoles,
            
            SystemUserRoles = userRoles,
            
            FilteredSystemRoles = KeycloakRoleSyncService.FilterSystemRoles(clientRoles),
            
            TokenInfo = new
            {
                TotalClaims = User.Claims.Count(),
                HasResourceAccess = User.HasClaim(c => c.Type == "resource_access"),
                ClientId = User.FindFirstValue("aud")
            },
            
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        return Ok(tokenInfo);
    }

    [HttpPost("sync-client-roles")]
    public async Task<IActionResult> SyncClientRoles()
    {
        var user = await UserService.GetUserByPrincipalAsync(User);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        var result = await KeycloakRoleSyncService.SyncUserRolesFromTokenAsync(user, User);
        
        if (result.IsSuccess)
        {
            var updatedUserRoles = await RoleService.GetUserRolesAsync(user);
            var clientRoles = User.GetClientRoles();
            
            return Ok(new
            {
                Message = "Client rol senkronizasyonu başarılı",
                UpdatedSystemRoles = updatedUserRoles,
                ClientRoles = clientRoles,
                FilteredSystemRoles = KeycloakRoleSyncService.FilterSystemRoles(clientRoles),
                SyncedAt = DateTime.UtcNow
            });
        }

        return BadRequest(new
        {
            Message = "Client rol senkronizasyonu başarısız",
            Errors = result.Errors
        });
    }
} 