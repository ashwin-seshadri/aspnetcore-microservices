using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mango.Web.Models;
using Mango.Web.Services.Interfaces;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
	public class AuthController : Controller
	{
		private readonly IAuthService _authService;
		private readonly ITokenProvider _tokenProvider;
		public AuthController(IAuthService authService, ITokenProvider tokenProvider)
		{
			_authService = authService;
			_tokenProvider = tokenProvider;
		}

		[HttpGet]
		public IActionResult Login()
		{
			var loginRequestDto = new LoginRequestDto();
			return View(loginRequestDto);
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginRequestDto model)
		{
			ResponseDto responseDto = await _authService.LoginAsync(model);
			if (responseDto != null && responseDto.IsSuccess)
			{
				var loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));
				await SignInUser(loginResponseDto);
				_tokenProvider.SetToken(loginResponseDto.Token);
				return RedirectToAction("Index", "Home");
			}
			else
			{
				TempData["error"] = responseDto?.Message;
				return View(model);
			}
		}

		[HttpGet]
		public IActionResult Register()
		{
			var roleList = new List<SelectListItem>
			{
				new SelectListItem { Text = StaticDetails.RoleAdmin, Value = StaticDetails.RoleAdmin },
				new SelectListItem { Text = StaticDetails.RoleCustomer, Value = StaticDetails.RoleCustomer }
			};

			ViewBag.RoleList = roleList;

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegistrationDto model)
		{
			ResponseDto result = await _authService.RegisterAsync(model);
			ResponseDto roleResult;
			if (result != null && result.IsSuccess)
			{
				if (string.IsNullOrEmpty(model.Role))
				{
					model.Role = StaticDetails.RoleCustomer;
				}

				roleResult = await _authService.AssignRoleAsync(model);
				if (roleResult != null && roleResult.IsSuccess)
				{
					TempData["success"] = "Registration successful";
					return RedirectToAction(nameof(Login));
				}
			}
			else
			{
				TempData["error"] = result?.Message;
			}

			var roleList = new List<SelectListItem>
			{
				new SelectListItem { Text = StaticDetails.RoleAdmin, Value = StaticDetails.RoleAdmin },
				new SelectListItem { Text = StaticDetails.RoleCustomer, Value = StaticDetails.RoleCustomer }
			};

			ViewBag.RoleList = roleList;
			return View(model);
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			_tokenProvider.RemoveToken();
			return RedirectToAction("Index", "Home");
		}

		private async Task SignInUser(LoginResponseDto model)
		{
			var handler = new JwtSecurityTokenHandler();

			var jwt = handler.ReadJwtToken(model.Token);

			var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
			identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
				jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Email).Value));
			identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
				jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
			identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
				jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Name).Value));
			// Add microsoft identity claim
			identity.AddClaim(new Claim(ClaimTypes.Name,
				jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Email).Value));
			identity.AddClaim(new Claim(ClaimTypes.Role,
				jwt.Claims.First(u => u.Type == "role").Value));

			var principal = new ClaimsPrincipal(identity);
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
		}
	}
}
