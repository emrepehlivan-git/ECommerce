using System.Security.Claims;

namespace ECommerce.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return subjectId is null ? Guid.Empty : Guid.Parse(subjectId);
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email);
    }

    public static string? GetFirstName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.GivenName);
    }

    public static string? GetLastName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Surname);
    }

    public static string GetFullName(this ClaimsPrincipal principal)
    {
        var firstName = GetFirstName(principal);
        var lastName = GetLastName(principal);
        
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return principal.FindFirstValue("name") ?? string.Empty;
        }

        return $"{firstName} {lastName}";
    }
} 