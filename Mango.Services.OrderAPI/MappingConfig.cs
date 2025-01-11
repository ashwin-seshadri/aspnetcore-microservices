using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
                {
                    config.CreateMap<CartHeaderDto, OrderHeaderDto>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore());
                    config.CreateMap<CartDetailDto, OrderDetailDto>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                        .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));

                    config.CreateMap<OrderHeaderDto, OrderHeader>().ReverseMap();
                    config.CreateMap<OrderDetailDto, OrderDetail>().ReverseMap();
                });
            return mapperConfiguration;
        }
    }
}
