namespace ECommerce.Application.Features.Carts;

public static class CartConsts
{
    public const int MaxItemsInCart = 50;
    public const int MaxQuantityPerItem = 999;
    public const decimal MaxTotalAmount = 999999.99m;
    
    public static class ErrorMessages
    {
        public const string CartNotFound = "Cart:NotFound";
        public const string CartItemNotFound = "Cart:ItemNotFound";
        public const string ProductNotFound = "Product:NotFound";
        public const string ProductNotActive = "Product:NotActive";
        public const string InsufficientStock = "Cart:InsufficientStock";
        public const string MaxItemsExceeded = "Cart:MaxItemsExceeded";
        public const string MaxQuantityExceeded = "Cart:MaxQuantityExceeded";
        public const string MaxTotalAmountExceeded = "Cart:MaxTotalAmountExceeded";
        public const string InvalidQuantity = "Cart:InvalidQuantity";
        public const string InvalidProductId = "Cart:InvalidProductId";
        public const string InvalidUserId = "Cart:InvalidUserId";
    }
    
    public static class ValidationMessages
    {
        public const string QuantityRequired = "Cart:Validation:QuantityRequired";
        public const string QuantityMustBePositive = "Cart:Validation:QuantityMustBePositive";
        public const string ProductIdRequired = "Cart:Validation:ProductIdRequired";
        public const string UserIdRequired = "Cart:Validation:UserIdRequired";
    }
} 