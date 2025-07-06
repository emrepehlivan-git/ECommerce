namespace ECommerce.Application.Services;

public interface IKeycloakPermissionSyncService
{
    Task SyncPermissionsToKeycloakAsync();
    Task AssignPermissionsToKeycloakUserAsync(string userId, IEnumerable<string> permissions);
} 