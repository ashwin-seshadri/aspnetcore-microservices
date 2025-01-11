using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService,IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await LoadCartBasedOnLoggedInUser());
        }

        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartBasedOnLoggedInUser());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            var cart = await LoadCartBasedOnLoggedInUser();
            cart.CartHeader.Name = cartDto.CartHeader.Name;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;
            
            var response = await _orderService.CreateOrder(cart);
            if (response != null && response.IsSuccess)
            {
                var orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var StripeRequestDto = new StripeRequestDto
                {
                    SuccessUrl = domain + $"cart/confirmation?orderId={orderHeader.Id}",
                    CancelUrl = domain + "cart/Checkout",
                    OrderHeader = orderHeader
                };

                var stripeResponse = await _orderService.CreateStripeSession(StripeRequestDto);
                if (stripeResponse != null && stripeResponse.IsSuccess)
                {
                    var stripeRequest = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));
                    Response.Headers.Add("Location", stripeRequest.StripeSessionUrl);
                    return new StatusCodeResult(303);
                }
            }

            return View();
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var response = await _orderService.ValidateStripeSession(orderId);
            if (response != null && response.IsSuccess)
            {
                var orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if(orderHeader.Status == StaticDetails.Status_Approved)
                {
                    return View(orderId);
                }
                //TODO: redirect based on error message
            }
            TempData["error"] = "Something went wrong in the payment.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartDetailId)
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
            var response = await _cartService.RemoveFromCartAsync(cartDetailId);
            if(response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon applied successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            cartDto.CartHeader.CouponCode = "";
            var response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon applied successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            var cart = await LoadCartBasedOnLoggedInUser();
            cart.CartHeader.Email = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value;
            var response = await _cartService.EmailCartAsync(cart);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and sent shortly.";
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Index));
        }

        private async Task<CartDto> LoadCartBasedOnLoggedInUser()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
            var response = await _cartService.GetCartByUserIdAsync(userId);
            if (response != null && response.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
            }

            return new CartDto();
        }
    }
}
