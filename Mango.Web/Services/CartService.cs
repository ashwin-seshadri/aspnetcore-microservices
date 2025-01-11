using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;
        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = ShoppingCartApiBase + "/api/cart/ApplyCoupon"
            });
        }

        public async Task<ResponseDto?> EmailCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = ShoppingCartApiBase + "/api/cart/EmailCartRequest"
            });
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = ShoppingCartApiBase + $"/api/cart/GetCart/{userId}"
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = ShoppingCartApiBase + $"/api/cart/RemoveCartDetail",
                Data = cartDetailsId
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = ShoppingCartApiBase + "/api/cart/CartUpsert"
            });
        }
    }
}
