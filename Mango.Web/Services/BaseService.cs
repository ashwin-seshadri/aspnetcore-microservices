using System.Net;
using System.Text;
using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Newtonsoft.Json;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Services
{
	public class BaseService : IBaseService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ITokenProvider _tokenProvider;
		public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) 
		{
			_httpClientFactory = httpClientFactory;
			_tokenProvider = tokenProvider;
		}

		public async Task<ResponseDto?> SendAsync(RequestDto request, bool withBearer = true)
		{
			try
			{
				var client = _httpClientFactory.CreateClient("MangoApi");
				HttpRequestMessage message = new();
				if (request.ContentType == ContentType.MultipartFormData)
				{
					message.Headers.Add("Accept", "*/*");
				}
				else
				{
					message.Headers.Add("Accept", "application/json");
				}

				//token
				if(withBearer)
				{
					var token = _tokenProvider.GetToken();
					message.Headers.Add("Authorization", $"Bearer {token}");
				}

				message.RequestUri = new Uri(request.Url);

				if (request.ContentType == ContentType.MultipartFormData) 
				{
					var content = new MultipartFormDataContent();
					foreach (var prop in request.Data.GetType().GetProperties())
					{
						var value = prop.GetValue(request.Data);
						if (value is FormFile)
						{
							var file = (FormFile)value;
							if (file != null)
							{
								content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
							}
						}
						else
						{
							content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
						}
					}
					message.Content = content;
				}
				else
				{
                    if (request.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
                    }
                }

				switch (request.ApiType)
				{
					case ApiType.POST:
						message.Method = HttpMethod.Post;
						break;
					case ApiType.PUT:
						message.Method = HttpMethod.Put;
						break;
					case ApiType.DELETE:
						message.Method = HttpMethod.Delete;
						break;
					default:
						message.Method = HttpMethod.Get;
						break;
				}

				HttpResponseMessage? response = await client.SendAsync(message);

				switch (response.StatusCode)
				{
					case HttpStatusCode.NotFound:
						return new() { IsSuccess = false, Message = "Not Found" };
					case HttpStatusCode.Forbidden:
						return new() { IsSuccess = false, Message = "Access Denied" };
					case HttpStatusCode.Unauthorized:
						return new() { IsSuccess = false, Message = "Unauthorized" };
					case HttpStatusCode.InternalServerError:
						return new() { IsSuccess = false, Message = "Internal Server Error" };
					case HttpStatusCode.NoContent:
                        return new() { IsSuccess = true };
                    default:
						var content = await response.Content.ReadAsStringAsync();
						var responseDto = JsonConvert.DeserializeObject<ResponseDto>(content);
						return responseDto;

				}
			}
			catch (Exception ex)
			{
				return new() { IsSuccess = false, Message = ex.Message };
			}
		}
	}
}