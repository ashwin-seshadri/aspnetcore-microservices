namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
	public class CouponDto
	{
		public int Id { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public double DiscountAmount { get; set; }
		public int MinimumAmount { get; set; }
	}
}
