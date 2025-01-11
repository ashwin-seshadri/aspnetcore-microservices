using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Services
{
	public class AuthService : IAuthService
	{
		private readonly AppDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IJwtTokenGenerator _jwtTokenGenerator;

		public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
			_jwtTokenGenerator = jwtTokenGenerator;
		}

		public async Task<bool> AssignRole(string email, string roleName)
		{
			var user = this._db.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
			if(user != null)
			{
				if(!this._roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
				{
					this._roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
				}
				await _userManager.AddToRoleAsync(user, roleName);
				return true;
			}

			return false;
		}

		public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
		{
			var user = this._db.Users.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

			var failed = new LoginResponseDto { User = null, Token = "" };

			if (user != null)
			{
				var isValid = await this._userManager.CheckPasswordAsync(user, loginRequestDto.Password);

				if (!isValid)
				{
					return failed;
				}

				// User was found and password was valid, so generate the token
				var roles = await _userManager.GetRolesAsync(user);
				var token = _jwtTokenGenerator.GenerateToken(user, roles);

				// return the user and token in login response
				var userDto = new UserDto
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email,
					PhoneNumber = user.PhoneNumber
				};

				return new LoginResponseDto { User = userDto, Token = token };
			}

			return failed;
		}

		public async Task<string> Register(RegistrationDto registrationDto)
		{
			var user = new ApplicationUser
			{
				UserName = registrationDto.Email,
				Email = registrationDto.Email,
				NormalizedEmail = registrationDto.Email.ToUpper(),
				Name = registrationDto.Name,
				PhoneNumber = registrationDto.PhoneNumber
			};

			try 
			{
				var result = await _userManager.CreateAsync(user, registrationDto.Password);
				if (result.Succeeded)
				{
					var userToReturn = _db.Users.First(u => u.UserName == registrationDto.Email);

					var userDto = new UserDto
					{
						Id = userToReturn.Id,
						Name = userToReturn.Name,
						Email = userToReturn.Email,
						PhoneNumber = userToReturn.PhoneNumber
					};

					return "";
				}
				else
				{
					return result.Errors.FirstOrDefault().Description;
				}
			}
			catch (Exception ex)
			{
				return $"Error encountered while creating user {registrationDto.Email}: {ex.Message}";
			}
		}
	}
}
