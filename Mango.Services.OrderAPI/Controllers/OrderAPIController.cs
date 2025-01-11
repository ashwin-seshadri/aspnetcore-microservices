using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Services.Interfaces;
using Mango.Services.OrderAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Authorize]
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly ResponseDto _responseDto;
        private readonly IProductService _productService;
        private readonly IMessageBus _messageBus;

        public OrderAPIController(IConfiguration configuration, IMapper mapper, AppDbContext db, IProductService productService, IMessageBus messageBus)
        {
            _configuration = configuration;
            _mapper = mapper;
            _db = db;
            _productService = productService;
            _messageBus = messageBus;
            _responseDto = new ResponseDto();
        }

        [HttpGet("GetOrders")]
        public async Task<IActionResult> GetOrders(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;
                if (User.IsInRole(StaticDetails.RoleAdmin))
                {
                    orderHeaders = await _db.OrderHeaders
                        .Include(o => o.OrderDetails)
                        .OrderByDescending(o => o.Id)
                        .ToListAsync();
                }
                else
                {
                    orderHeaders = await _db.OrderHeaders
                        .Include(o => o.OrderDetails)
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.Id).ToListAsync();
                }
                _responseDto.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orderHeaders);
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }

        [HttpGet("GetOrder/{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var orderHeader = await _db.OrderHeaders.Include(o => o.OrderDetails).FirstAsync(o => o.Id == id);
                _responseDto.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                var orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.CreatedAt = DateTime.Now;
                orderHeaderDto.Status = StaticDetails.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<List<OrderDetailDto>>(cartDto.CartDetails);

                var orderCreated = (await _db.OrderHeaders.AddAsync(_mapper.Map<OrderHeader>(orderHeaderDto))).Entity;
                await _db.SaveChangesAsync();
                orderHeaderDto.Id = orderCreated.Id;
                _responseDto.Result = orderHeaderDto;
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }

        [HttpPost("CreateStripeSession")]
        public async Task<IActionResult> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.SuccessUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                var discounts = new List<SessionDiscountOptions>
                {
                    new SessionDiscountOptions
                    {
                        Coupon = stripeRequestDto.OrderHeader.CouponCode
                    }
                };

                foreach(var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            },
                            UnitAmount = (long)(item.Price * 100),
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                if(stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts = discounts;
                }

                var service = new SessionService();
                var session = service.Create(options);
                stripeRequestDto.StripeSessionId = session.Id;
                stripeRequestDto.StripeSessionUrl = session.Url;

                var orderHeader = await _db.OrderHeaders.FirstAsync(o => o.Id == stripeRequestDto.OrderHeader.Id);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();
                _responseDto.Result = stripeRequestDto;
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }

        [HttpPost("ValidateStripeSession")]
        public async Task<IActionResult> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                var orderHeader = await _db.OrderHeaders.FirstAsync(o => o.Id == orderHeaderId);

                var sessionService = new SessionService();
                var session = sessionService.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if(paymentIntent.Status == "succeeded")
                {
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = StaticDetails.Status_Approved;
                    await _db.SaveChangesAsync();

                    RewardDto rewardDto = new()
                    {
                        OrderId = orderHeader.Id,
                        RewardActivity = Convert.ToInt32(orderHeader.Total),
                        UserId = orderHeader.UserId
                    };

                    var topicName = _configuration.GetValue<string>("TopicsAndQueueNames:OrderCreatedTopic");
                    await _messageBus.PublishMessage(rewardDto, topicName);
                }

                _responseDto.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }

        [HttpPost("UpdateOrderStatus/{id:int}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                var orderHeader = await _db.OrderHeaders.FirstAsync(o => o.Id == id);
                if(newStatus == StaticDetails.Status_Cancelled)
                {
                    // we will provide refund
                    var options = new RefundCreateOptions
                    {
                        PaymentIntent = orderHeader.PaymentIntentId,
                        Reason = RefundReasons.RequestedByCustomer
                    };

                    var service = new RefundService();
                    var refund = service.Create(options);
                    
                }
                
                orderHeader.Status = newStatus;
                await _db.SaveChangesAsync();
                _responseDto.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                return Ok(_responseDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { IsSuccess = false, Message = e.Message });
            }
        }
    }
}
