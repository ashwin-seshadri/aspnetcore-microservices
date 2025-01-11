namespace Mango.Web.Services.Interfaces
{
	public interface ITokenProvider
	{
		void SetToken(string token);
		string? GetToken();
		void RemoveToken();
	}
}
