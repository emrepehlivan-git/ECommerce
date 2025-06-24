namespace ECommerce.Application.Features.UserAddresses;

public static class UserAddressConsts
{
    public const string NotFound = "UserAddress:NotFound";
    public const string UserNotFound = "UserAddress:UserNotFound";
    public const string LabelRequired = "UserAddress:LabelRequired";
    public const string LabelMinLength = "UserAddress:LabelMinLength";
    public const string LabelMaxLength = "UserAddress:LabelMaxLength";
    public const string AddressRequired = "UserAddress:AddressRequired";
    public const string DefaultAddressCannotBeDeleted = "UserAddress:DefaultAddressCannotBeDeleted";
    public const string AddressAlreadyDefault = "UserAddress:AddressAlreadyDefault";
    
    // Address fields validation
    public const string StreetRequired = "UserAddress:StreetRequired";
    public const string StreetMaxLength = "UserAddress:StreetMaxLength";
    public const string CityRequired = "UserAddress:CityRequired";
    public const string CityMaxLength = "UserAddress:CityMaxLength";
    public const string ZipCodeRequired = "UserAddress:ZipCodeRequired";
    public const string ZipCodeMaxLength = "UserAddress:ZipCodeMaxLength";
    public const string CountryRequired = "UserAddress:CountryRequired";
    public const string CountryMaxLength = "UserAddress:CountryMaxLength";
    
    public const int LabelMinLengthValue = 2;
    public const int LabelMaxLengthValue = 50;
    public const int StreetMaxLengthValue = 200;
    public const int CityMaxLengthValue = 100;
    public const int ZipCodeMaxLengthValue = 20;
    public const int CountryMaxLengthValue = 100;
} 