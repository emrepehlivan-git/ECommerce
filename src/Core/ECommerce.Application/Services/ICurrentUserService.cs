namespace ECommerce.Application.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Name { get; }
    string? Role { get; }

    IEnumerable<string> GetPermissions();
    bool HasPermission(string permission);
}
