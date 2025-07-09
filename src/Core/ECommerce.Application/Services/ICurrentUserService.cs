namespace ECommerce.Application.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Name { get; }
    List<string> Roles { get; }

    IEnumerable<string> GetPermissions();
    bool HasPermission(string permission);
}
