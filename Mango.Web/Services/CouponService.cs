using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;

namespace Mango.Web.Services
{
	public class CouponService : ICouponService
	{
		private readonly IBaseService _baseService;
		public CouponService(IBaseService baseService)
		{
			_baseService = baseService;
		}

		public async Task<ResponseDto?> CreateCouponAsync(CouponDto couponDto)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.POST,
				Data = couponDto,
				Url = StaticDetails.CouponApiBase + $"/api/coupon"
			});
		}

		public async Task<ResponseDto?> DeleteCouponAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.DELETE,
				Url = StaticDetails.CouponApiBase + $"/api/coupon/{id}"
			});
		}

		public async Task<ResponseDto?> GetAllCouponsAsync()
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.GET,
				Url = StaticDetails.CouponApiBase + "/api/coupon"
			});
		}

		public async Task<ResponseDto?> GetCouponByCodeAsync(string couponCode)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.GET,
				Url = StaticDetails.CouponApiBase + $"/api/coupon/GetByCode/{couponCode}"
			});
		}

		public async Task<ResponseDto?> GetCouponByIdAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.GET,
				Url = StaticDetails.CouponApiBase + $"/api/coupon/{id}"
			});
		}

		public async Task<ResponseDto?> UpdateCouponAsync(CouponDto couponDto)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.PUT,
				Data = couponDto,
				Url = StaticDetails.CouponApiBase + $"/api/coupon"
			});
		}
	}
}
