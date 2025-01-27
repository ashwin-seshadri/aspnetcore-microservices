﻿using System.ComponentModel.DataAnnotations;

namespace Mango.Services.CouponAPI.Models
{
	public class Coupon
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string CouponCode { get; set; } = string.Empty;
		[Required]
		public double DiscountAmount { get; set; }
		public int MinimumAmount { get; set; }
	}
}
