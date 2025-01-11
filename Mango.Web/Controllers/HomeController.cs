using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
	public class HomeController : Controller
	{
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(IProductService productService, ICartService cartService)
		{
            _productService = productService;
            _cartService = cartService;
        }

		public async Task<IActionResult> Index()
		{
            List<ProductDto>? list = new();
            ResponseDto? response = await _productService.GetAllProductsAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }

        [Authorize]
        public async Task<IActionResult> ProductDetails(int id)
        {
            ProductDto? model = new();
            ResponseDto? response = await _productService.GetProductByIdAsync(id);
            if (response != null && response.IsSuccess)
            {
                model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProductDetails(ProductDto product)
        {
            var cartDto = new CartDto
            {
                CartHeader = new CartHeaderDto
                {
                    UserId = User.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value
                },
                CartDetails = new List<CartDetailDto>
                {
                    new CartDetailDto
                    {
                        Count = product.Count,
                        ProductId = product.Id
                    }
                }
            };

            var response = await _cartService.UpsertCartAsync(cartDto);
            if(response != null && response.IsSuccess)
            {
                TempData["success"] = "Item has been added to the cart successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(product);
        }

        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
