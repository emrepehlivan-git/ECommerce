namespace ECommerce.Application.Features.Roles;

public static class RoleConsts
{
    // Name Validation
    public const string NameIsRequired = "Role_Name_Required";
    public const string NameExists = "Role_Name_Exists";
    public const string NameMustBeAtLeastCharacters = "Role_Name_AtLeast_Characters";
    public const string NameMustBeLessThanCharacters = "Role_Name_LessThan_Characters";
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;

    // General Messages
    public const string RoleNotFound = "Role_Not_Found";
    public const string UserNotFound = "User_Not_Found";
    public const string RoleAddedToUser = "Role_Added_To_User";
    public const string RoleRemovedFromUser = "Role_Removed_From_User";
    public const string UserAlreadyInRole = "User_Already_In_Role";
    public const string UserNotInRole = "User_Not_In_Role";
} 