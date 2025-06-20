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
    
    public const int LabelMinLengthValue = 2;
    public const int LabelMaxLengthValue = 50;
} 