using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mango.Services.CouponAPI.Controllers
{
	[Route("api/coupon")]
	[ApiController]
	[Authorize]
	public class CouponAPIController : ControllerBase
	{
		private AppDbContext _db;
		private IMapper _mapper;
		private ResponseDto _response;

		public CouponAPIController(AppDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
			_response = new ResponseDto();
		}

		[HttpGet]
		public ResponseDto Get()
		{
			try
			{
				_response.Result = _mapper.Map<IEnumerable<CouponDto>>(_db.Coupons.ToList());
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}
			return _response;
		}

		[HttpGet]
		[Route("{id:int}")]
		public ResponseDto Get(int id)
		{
			try
			{
				var coupon = _db.Coupons.FirstOrDefault(x => x.Id == id);
				if(coupon == null)
				{
					_response.IsSuccess = false;
					_response.Message = "Coupon not found";
				}
				else
				{
					_response.Result = _mapper.Map<CouponDto>(coupon);
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}

			return _response;
		}

		[HttpGet]
		[Route("GetByCode/{code}")]
		public ResponseDto GetByCode(string code)
		{
			try
			{
				var coupon = _db.Coupons.FirstOrDefault(x => x.CouponCode.ToLower() == code.ToLower());
				if (coupon == null)
				{
					_response.IsSuccess = false;
					_response.Message = "Coupon not found";
				}
				else
				{
					_response.Result = _mapper.Map<CouponDto>(coupon);
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}

			return _response;
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Post([FromBody] CouponDto coupon)
		{
			try
			{
				var obj = _mapper.Map<Coupon>(coupon);
				_db.Coupons.Add(obj);
				_db.SaveChanges();

                var options = new Stripe.CouponCreateOptions
                {
                    AmountOff = (long)(coupon.DiscountAmount*100),
                    Name = coupon.CouponCode,
                    Currency = "usd",
                    Id = coupon.CouponCode
                };
                var service = new Stripe.CouponService();
                service.Create(options);
                _response.Result = _mapper.Map<CouponDto>(obj);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}

			return _response;
		}

		[HttpPut]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Put([FromBody] CouponDto coupon)
		{
			try
			{
				var obj = _mapper.Map<Coupon>(coupon);
				_db.Coupons.Update(obj);
				_db.SaveChanges();
				_response.Result = _mapper.Map<CouponDto>(obj);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}

			return _response;
		}

		[HttpDelete]
		[Route("{id:int}")]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Delete(int id)
		{
			try
			{
				var coupon = _db.Coupons.FirstOrDefault(x => x.Id == id);
				if(coupon != null)
				{
					_db.Coupons.Remove(coupon);
					_db.SaveChanges();

                    var service = new Stripe.CouponService();
                    service.Delete(coupon.CouponCode);
                }
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}

			return _response;
		}
	}
}
