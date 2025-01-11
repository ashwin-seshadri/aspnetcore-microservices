using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;
using Newtonsoft.Json.Linq;

namespace Mango.Web.Services
{
	public class TokenProvider : ITokenProvider
	{
		private readonly IHttpContextAccessor _contextAccessor;

		public TokenProvider(IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
		}

		public string? GetToken()
		{
			string? token = null;
			_contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetails.TokenCookie, out token);
			return token;
		}

		public void RemoveToken()
		{
			_contextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.TokenCookie);
		}

		public void SetToken(string token)
		{
			_contextAccessor.HttpContext?.Response.Cookies.Append(StaticDetails.TokenCookie, token);
		}
	}
}
