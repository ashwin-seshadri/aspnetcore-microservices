using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthAPIController : ControllerBase
	{
		private readonly IAuthService _authService;
        private readonly IMessageBus _messageBus;
		private readonly IConfiguration _configuration;
        private ResponseDto _response;

		public AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
		{
			_authService = authService;
			_messageBus = messageBus;
			_configuration = configuration;
			_response = new();
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto)
		{
			var errorMessage = await _authService.Register(registrationDto);
			if(!string.IsNullOrEmpty(errorMessage))
			{
				_response.IsSuccess = false;
				_response.Message = errorMessage;
				return BadRequest(_response);
			}

            await _messageBus.PublishMessage(registrationDto.Email, _configuration.GetValue<string>("TopicsAndQueueNames:RegisterUserQueue"));
            return Ok(_response);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
		{
			var loginResponseDto = await _authService.Login(loginRequestDto);
			if(loginResponseDto.User == null)
			{
				_response.IsSuccess = false;
				_response.Message = "Username or password incorrect";
				return BadRequest(_response);
			}
			_response.Result = loginResponseDto;
			return Ok(_response);
		}

		[HttpPost("assignrole")]
		public async Task<IActionResult> AssignRole([FromBody] RegistrationDto registrationDto)
		{
			var successful = await _authService.AssignRole(registrationDto.Email, registrationDto.Role.ToUpper());
			if (!successful)
			{
				_response.IsSuccess = false;
				_response.Message = "Error encountered. Unable to assign role.";
				return BadRequest(_response);
			}

			return Ok(_response);
		}
	}
}
