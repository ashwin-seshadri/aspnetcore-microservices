using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly ResponseDto _responseDto;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IMessageBus _messageBus;

        public CartAPIController(IConfiguration configuration, IMapper mapper, AppDbContext db, IProductService productService, ICouponService couponService, IMessageBus messageBus)
        {
            _configuration = configuration;
            _mapper = mapper;
            _db = db;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _responseDto = new ResponseDto();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
                if (cartHeaderFromDb != null)
                {
                    var cartDetailsFromDb = await _db.CartDetails.Where(cd => cd.CartHeaderId == cartHeaderFromDb.Id).ToListAsync();
                    var cartDto = new CartDto
                    {
                        CartHeader = _mapper.Map<CartHeaderDto>(cartHeaderFromDb),
                        CartDetails = _mapper.Map<IEnumerable<CartDetailDto>>(cartDetailsFromDb)
                    };

                    var products = await _productService.GetProductsAsync();

                    foreach (var item in cartDto.CartDetails)
                    {
                        item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        cartDto.CartHeader.Total += item.Product.Price * item.Count;
                    }

                    //apply coupon, if any
                    if(!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                    {
                        var coupon = await _couponService.GetCouponAsync(cartDto.CartHeader.CouponCode);
                        if (coupon != null && cartDto.CartHeader.Total >= coupon.MinimumAmount)
                        {
                            cartDto.CartHeader.Total -= coupon.DiscountAmount;
                            cartDto.CartHeader.Discount = coupon.DiscountAmount;
                        }
                    }

                    _responseDto.Result = cartDto;
                }
                else
                {
                    _responseDto.Result = new CartDto
                    {
                        CartHeader = new CartHeaderDto
                        {
                            UserId = userId
                        },
                        CartDetails = new List<CartDetailDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
            }
            return Ok(_responseDto);
        }

        [HttpPost("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstAsync(c => c.UserId == cartDto.CartHeader.UserId);
                cartHeaderFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _db.CartHeaders.Update(cartHeaderFromDb);
                await _db.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
            }
            return Ok(_responseDto);
        }

        [HttpPost("EmailCartRequest")]
        public async Task<IActionResult> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicsAndQueueNames:EmailShoppingCartQueue"));
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
            }
            return Ok(_responseDto);
        }

        [HttpPost("CartUpsert")]
        public async Task<IActionResult> CartUpsert([FromBody] CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    //create card header and details
                    var cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    await _db.CartHeaders.AddAsync(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.Id;
                    await _db.CartDetails.AddAsync(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                    _responseDto.Result = cartDto;
                }
                else
                {
                    //check for existing card detail with same product id and cart header id
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(cd => cd.ProductId == cartDto.CartDetails.First().ProductId 
                    && cd.CartHeaderId == cartHeaderFromDb.Id);
                    if (cartDetailsFromDb == null)
                    {
                        //create cart details
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.Id;
                        await _db.CartDetails.AddAsync(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                        _responseDto.Result = cartDto;
                    }
                    else
                    {
                        //update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().Id = cartDetailsFromDb.Id;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        _db.CartDetails.Update(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                        _responseDto.Result = cartDto;
                    }
                }
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
            }
            return Ok(_responseDto);
        }

        [HttpPost("RemoveCartDetail")]
        public async Task<IActionResult> RemoveCartDetail([FromBody] int cartDetailId)
        {
            try
            {
                var cartDetailFromDb = await _db.CartDetails.FirstAsync(c => c.Id == cartDetailId);

                var numberOfCartDetails = _db.CartDetails.Count(cd => cd.CartHeaderId == cartDetailFromDb.CartHeaderId);
                _db.CartDetails.Remove(cartDetailFromDb);

                if (numberOfCartDetails == 1)
                {
                    var cartHeaderFromDb = await _db.CartHeaders.FirstAsync(c => c.Id == cartDetailFromDb.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeaderFromDb);
                }

                await _db.SaveChangesAsync();

                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
            }
            return Ok(_responseDto);
        }
    }
}
