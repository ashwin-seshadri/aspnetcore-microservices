using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;

namespace Mango.Web.Services
{
	public class AuthService : IAuthService
	{
		private readonly IBaseService _baseService;
		public AuthService(IBaseService baseService)
		{
			_baseService = baseService;
		}

		public async Task<ResponseDto?> AssignRoleAsync(RegistrationDto registrationDto)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.POST,
				Data = registrationDto,
				Url = StaticDetails.AuthApiBase + $"/api/auth/assignrole"
			});
		}

		public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.POST,
				Data = loginRequestDto,
				Url = StaticDetails.AuthApiBase + $"/api/auth/login"
			}, withBearer: false);
		}

		public async Task<ResponseDto?> RegisterAsync(RegistrationDto registrationDto)
		{
			return await _baseService.SendAsync(new RequestDto
			{
				ApiType = StaticDetails.ApiType.POST,
				Data = registrationDto,
				Url = StaticDetails.AuthApiBase + $"/api/auth/register"
			}, withBearer: false);
		}
	}
}
