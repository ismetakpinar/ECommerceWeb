namespace ECommerce.Web.Infrastructure.Http;

public static class ApiEndpoints
{
    private const string Api = "api/v1";

    public static class Cart
    {
        public const string Base = $"{Api}/Cart";

        public const string Get = Base;

        public const string Add = $"{Base}/add";

        public const string Update = $"{Base}/update";

        public const string Remove = Base;

        public const string Checkout = $"{Base}/checkout";
    }

    public static class Product
    {
        public const string Base = $"{Api}/Products";
    }

    public static class Category
    {
        public const string Base = $"{Api}/Categories";
    }

    public static class Auth
    {
        public const string Base = $"{Api}/Auth";

        public const string Login = $"{Base}/login";

        public const string Register = $"{Base}/register";

        public const string RefreshToken = $"{Base}/refresh-token";

        public const string Logout = $"{Base}/logout";
    }



    public static class Orders
    {
        public const string Base = $"{Api}/Orders";

        public const string MyOrders = $"{Base}/my-orders";

        public const string MyOrderDetail = $"{Base}/my-orders/{{0}}";
    }
    public static class Addresses
    {
        public const string Base = $"{Api}/Addresses";

        public const string GetAll = Base;
    }
}