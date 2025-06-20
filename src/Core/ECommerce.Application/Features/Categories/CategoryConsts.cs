namespace ECommerce.Application.Features.Categories;

public static class CategoryConsts
{
    public const string NameExists = "Category:Name:Exists";
    public const string NotFound = "Category:NotFound";
    public const string NameIsRequired = "Category:Name:IsRequired";
    public const string NameMustBeAtLeastCharacters = "Category:Name:MustBeAtLeastCharacters";
    public const string NameMustBeLessThanCharacters = "Category:Name:MustBeLessThanCharacters";
    public const string CannotDeleteWithProducts = "Category:CannotDeleteWithProducts";

    public const int NameMinLength = 3;
    public const int NameMaxLength = 100;
}

