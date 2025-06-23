namespace ECommerce.Application.Features.Roles;

public static class RoleConsts
{
    // Name Validation
    public const string NameIsRequired = "Role.Name.IsRequired";
    public const string NameExists = "Role.Name.Exists";
    public const string NameMustBeAtLeastCharacters = "Role.Name.MustBeAtLeastCharacters";
    public const string NameMustBeLessThanCharacters = "Role.Name.MustBeLessThanCharacters";
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;

    // General Messages
    public const string RoleNotFound = "Role.NotFound";
    public const string UserNotFound = "Role.UserNotFound";
    public const string RoleAddedToUser = "Role.AddedToUser";
    public const string RoleRemovedFromUser = "Role.RemovedFromUser";
    public const string UserAlreadyInRole = "Role.UserAlreadyInRole";
    public const string UserNotInRole = "Role.UserNotInRole";
} 