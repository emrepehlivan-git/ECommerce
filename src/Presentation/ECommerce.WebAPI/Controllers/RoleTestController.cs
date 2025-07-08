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
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IKeycloakRoleSyncService _keycloakRoleSyncService;

    public RoleTestController(
        IUserService userService,
        IRoleService roleService,
        IKeycloakRoleSyncService keycloakRoleSyncService)
    {
        _userService = userService;
        _roleService = roleService;
        _keycloakRoleSyncService = keycloakRoleSyncService;
    }

    /// <summary>
    /// Client rol bilgilerini ve kullanıcı rollerini gösterir
    /// </summary>
    [HttpGet("client-roles")]
    public async Task<IActionResult> GetClientRoleInfo()
    {
        var user = await _userService.GetUserByPrincipalAsync(User);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        var userRoles = await _roleService.GetUserRolesAsync(user);
        
        var tokenInfo = new
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName.ToString(),
            
            // Sadece client rolleri
            ClientRoles = User.GetClientRoles(),
            AllKeycloakRoles = User.GetAllKeycloakRoles(), // Artık sadece client rolleri döndürür
            
            // Sistemdeki roller
            UserRoles = userRoles,
            
            // Filtrelenmiş sistem rolleri
            FilteredSystemRoles = _keycloakRoleSyncService.FilterSystemRoles(User.GetAllKeycloakRoles()),
            
            // Token'dan hiç realm rolleri gelmemeli
            RealmRoles = User.GetRealmRoles(), // Boş olmalı veya default roller olmalı
            
            // Tüm claims (debug için)
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        return Ok(tokenInfo);
    }

    /// <summary>
    /// Manuel client rol senkronizasyonu tetikler
    /// </summary>
    [HttpPost("sync-client-roles")]
    public async Task<IActionResult> SyncClientRoles()
    {
        var user = await _userService.GetUserByPrincipalAsync(User);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        var result = await _keycloakRoleSyncService.SyncUserRolesFromTokenAsync(user, User);
        
        if (result.IsSuccess)
        {
            var updatedUserRoles = await _roleService.GetUserRolesAsync(user);
            return Ok(new
            {
                Message = "Client rol senkronizasyonu başarılı",
                UpdatedRoles = updatedUserRoles,
                ClientRoles = User.GetClientRoles(),
                AllKeycloakRoles = User.GetAllKeycloakRoles() // Sadece client rolleri
            });
        }

        return BadRequest(new
        {
            Message = "Client rol senkronizasyonu başarısız",
            Errors = result.Errors
        });
    }
} 