using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Services.Interfaces
{
	public interface IJwtTokenGenerator
	{
		string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
	}
}
