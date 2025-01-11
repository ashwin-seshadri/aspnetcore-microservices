using AutoMapper;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
namespace Mango.Services.CouponAPI
{
	public class MappingConfig
	{
		public static MapperConfiguration RegisterMaps()
		{
			MapperConfiguration mapperConfiguration = new MapperConfiguration(config =>
			{
				config.CreateMap<Coupon, CouponDto>();
				config.CreateMap<CouponDto, Coupon>();
			});
			return mapperConfiguration;
		}
	}
}
