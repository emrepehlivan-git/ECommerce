namespace ECommerce.Application.Constants;

public static class PermissionConstants
{
    public static class Products
    {
        public const string Create = "Products.Create";
        public const string Update = "Products.Update";
        public const string Delete = "Products.Delete";
        public const string Manage = "Products.Manage";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Create = "Orders.Create";
        public const string Update = "Orders.Update";
        public const string Delete = "Orders.Delete";
        public const string Manage = "Orders.Manage";
    }

    public static class Categories
    {
        public const string Create = "Categories.Create";
        public const string Update = "Categories.Update";
        public const string Delete = "Categories.Delete";
        public const string Manage = "Categories.Manage";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
        public const string Manage = "Users.Manage";
    }

    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
        public const string Manage = "Roles.Manage";
    }
}

public static class PermissionConsts
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int ModuleMaxLength = 50;
    public const int ActionMaxLength = 50;
}