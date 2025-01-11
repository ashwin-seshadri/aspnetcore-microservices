using Mango.Web.Models;

namespace Mango.Web.Services.Interfaces
{
	public interface IAuthService
	{
		Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto);
		Task<ResponseDto?> RegisterAsync(RegistrationDto registrationDto);
		Task<ResponseDto?> AssignRoleAsync(RegistrationDto registrationDto);
	}
}
