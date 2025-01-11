using Mango.Services.AuthAPI.Models.Dto;

namespace Mango.Services.AuthAPI.Services.Interfaces
{
	public interface IAuthService
	{
		Task<string> Register(RegistrationDto registrationDto);
		Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
		Task<bool> AssignRole(string email, string roleName);
	}
}
