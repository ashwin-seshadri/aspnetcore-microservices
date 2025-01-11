using AutoMapper;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;

namespace Mango.Services.ProductAPI
{
	public class MappingConfig
	{
		public static MapperConfiguration RegisterMaps()
		{
			var mapperConfiguration = new MapperConfiguration(config =>
				{
					config.CreateMap<Product, ProductDto>();
					config.CreateMap<ProductDto, Product>();
				});
			return mapperConfiguration;
		}
	}
}
