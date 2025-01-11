namespace Mango.Web.Utilities
{
	public class StaticDetails
	{
		public static string CouponApiBase { get; set; }
		public static string AuthApiBase { get; set; }
        public static string ProductApiBase { get; set; }
        public static string ShoppingCartApiBase { get; set; }
        public static string OrderApiBase { get; set; }

        public const string RoleAdmin = "ADMIN";
		public const string RoleCustomer = "CUSTOMER";
		public const string TokenCookie = "JwtToken";

		public enum ApiType
		{
			GET,
			POST,
			PUT,
			DELETE
		}

        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_ReadyForPickup = "ReadyForPickup";
        public const string Status_Completed = "Completed";
        public const string Status_Cancelled = "Cancelled";
        public const string Status_Refunded = "Refunded";

		public enum ContentType
		{
			Json,
			MultipartFormData
		}
    }
}
