namespace ECommerce.Application.Features.Orders;

public static class OrderConsts
{
    public const string NotFound = "Order:NotFound";
    public const string UserNotFound = "Order:UserNotFound";
    public const string ProductNotFound = "Order:ProductNotFound";
    public const string QuantityMustBeGreaterThanZero = "Order:QuantityMustBeGreaterThanZero";
    public const string OrderCannotBeModified = "Order:CannotBeModified";
    public const string OrderCannotBeCancelled = "Order:CannotBeCancelled";
    public const string OrderItemNotFound = "Order:ItemNotFound";
    public const string ShippingAddressRequired = "Order:ShippingAddressRequired";
    public const string ShippingAddressNotFound = "Order:ShippingAddressNotFound";
    public const string BillingAddressRequired = "Order:BillingAddressRequired";
    public const string BillingAddressNotFound = "Order:BillingAddressNotFound";
    public const string EmptyOrder = "Order:EmptyOrder";
    public const string InsufficientStock = "Order:InsufficientStock";
    public const string ProductNotActive = "Order:ProductNotActive";
}