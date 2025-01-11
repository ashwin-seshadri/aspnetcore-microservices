using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(string status)
        {
            IEnumerable<OrderHeaderDto> list;
            string userId = "";
            if (!User.IsInRole(StaticDetails.RoleAdmin))
            {
                userId = User.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            }

            var response = await _orderService.GetOrders(userId);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<IEnumerable<OrderHeaderDto>>(Convert.ToString(response.Result));
                switch (status)
                {
                    case "approved":
                        list = list.Where(o => o.Status == StaticDetails.Status_Approved);
                        break;
                    case "readyforpickup":
                        list = list.Where(o => o.Status == StaticDetails.Status_ReadyForPickup);
                        break;
                    case "cancelled":
                        list = list.Where(o => o.Status == StaticDetails.Status_Cancelled);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                list = new List<OrderHeaderDto>();
            }

            return Json(new { data = list });
        }

        public async Task<IActionResult> Detail(int orderId)
        {
            OrderHeaderDto orderHeader = new();
            var userId = User.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            var response = await _orderService.GetOrder(orderId);
            if (response != null && response.IsSuccess)
            {
                orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if (User.IsInRole(StaticDetails.RoleAdmin) || orderHeader.UserId == userId)
                {
                    return View(orderHeader);
                }
            }

            return NotFound();
        }

        [HttpPost("SetReadyForPickup")]
        public async Task<IActionResult> SetReadyForPickup(int orderId)
        {
            var response = await _orderService.UpdateOrderStatus(orderId, StaticDetails.Status_ReadyForPickup);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
            else
            {
                TempData["error"] = "Unable to update order status.";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
            
        }

        [HttpPost("Complete")]
        public async Task<IActionResult> Complete(int orderId)
        {
            var response = await _orderService.UpdateOrderStatus(orderId, StaticDetails.Status_Completed);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
            else
            {
                TempData["error"] = "Unable to update order status.";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
        }

        [HttpPost("Cancel")]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var response = await _orderService.UpdateOrderStatus(orderId, StaticDetails.Status_Cancelled);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
            else
            {
                TempData["error"] = "Unable to update order status.";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
        }
    }
}
