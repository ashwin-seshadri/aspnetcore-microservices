using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = OrderApiBase + "/api/order/CreateOrder"
            });
        }

        public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = stripeRequestDto,
                Url = OrderApiBase + "/api/order/CreateStripeSession"
            });
        }

        public async Task<ResponseDto?> GetOrder(int orderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = OrderApiBase + $"/api/order/GetOrder/{orderId}"
            });
        }

        public async Task<ResponseDto?> GetOrders(string? userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = OrderApiBase + $"/api/order/GetOrders/{userId}"
            });
        }

        public async Task<ResponseDto?> UpdateOrderStatus(int orderId, string status)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = status,
                Url = OrderApiBase + $"/api/order/UpdateOrderStatus/{orderId}"
            });
        }

        public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Data = orderHeaderId,
                Url = OrderApiBase + "/api/order/ValidateStripeSession"
            });
        }
    }
}
