using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Controllers
{
	[Route("api/product")]
	[ApiController]
	public class ProductAPIController : ControllerBase
	{
		private readonly AppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ResponseDto _responseDto;

		public ProductAPIController(AppDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
			_responseDto = new ResponseDto();
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			try
			{
				var products = await _db.Products.ToListAsync();
				_responseDto.Result = products;
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
			}
			
			return Ok(_responseDto);
		}

		[HttpGet]
		[Route("{id:int}")]
		public async Task<IActionResult> Get(int id)
		{
			try
			{
				var product = await _db.Products.SingleOrDefaultAsync(p => p.Id == id);
				if(product == null)
				{
					_responseDto.IsSuccess = false;
					_responseDto.Message = "Product not found";
					return NotFound(_responseDto);
				}
				_responseDto.Result = product;
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
			}

			return Ok(_responseDto);
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		public async Task<IActionResult> Post(ProductDto productDto)
		{
			if (productDto == null || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			try
			{
				var product = _mapper.Map<Product>(productDto);
				await _db.Products.AddAsync(product);
				await _db.SaveChangesAsync();

				if(productDto.Image != null)
				{
					var fileName = $"{product.Id}{Path.GetExtension(productDto.Image.FileName)}";
					var filePath = @$"wwwroot\ProductImages\{fileName}";
					var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
					using(var fileStream = new FileStream(filePathDirectory, FileMode.Create))
					{
                        productDto.Image.CopyTo(fileStream);
                    }

					var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
					product.ImageUrl = $"{baseUrl}/ProductImages/{fileName}";
					product.ImageLocalPath = filePath;
                }
				else
				{
					product.ImageUrl = "https://placehold.co/600x400";
				}

				_db.Products.Update(product);
				_db.SaveChanges();

				_responseDto.Result = _mapper.Map<ProductDto>(product); ;
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
			}
			return CreatedAtAction(nameof(Get), new { id = ((ProductDto)_responseDto.Result).Id }, _responseDto);
		}

		[HttpPut]
		[Authorize(Roles = "ADMIN")]
		public async Task<IActionResult> Put(ProductDto productDto)
		{
			if (productDto == null || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			try
			{
				var product = _mapper.Map<Product>(productDto);

                if (productDto.Image != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageLocalPath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                        var file = new FileInfo(oldFilePath);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    var fileName = $"{product.Id}{Path.GetExtension(productDto.Image.FileName)}";
                    var filePath = @$"wwwroot\ProductImages\{fileName}";
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        productDto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ImageUrl = $"{baseUrl}/ProductImages/{fileName}";
                    product.ImageLocalPath = filePath;
                }

                _db.Products.Update(product);
				await _db.SaveChangesAsync();
				_responseDto.Result = _mapper.Map<ProductDto>(product); ;
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
			}
			return Ok(_responseDto);
		}

		[HttpDelete]
		[Authorize(Roles = "ADMIN")]
		[Route("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var product = await _db.Products.FirstAsync(p => p.Id == id);
				if (!string.IsNullOrEmpty(product.ImageLocalPath))
				{
					var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
					var file = new FileInfo(oldFilePath);
					if (file.Exists)
					{
						file.Delete();
                    }
                }

				_db.Products.Remove(product);
				await _db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return StatusCode(StatusCodes.Status500InternalServerError, _responseDto);
			}
			return NoContent();
		}
	}
}
