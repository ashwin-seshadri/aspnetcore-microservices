using Mango.Web.Models;

namespace Mango.Web.Services.Interfaces
{
	public interface IBaseService
	{
		Task<ResponseDto?> SendAsync(RequestDto request, bool withBearer = true);
	}
}
