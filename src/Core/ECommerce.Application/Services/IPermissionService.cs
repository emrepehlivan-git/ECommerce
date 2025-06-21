namespace ECommerce.Application.Services;

    public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission);
    Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId);
}