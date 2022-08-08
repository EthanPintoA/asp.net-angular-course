using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class AccountController : BaseApiController
	{
		private readonly DataContext context;
		private readonly ITokenService tokenService;

		public AccountController(DataContext context, ITokenService tokenService)
		{
			this.tokenService = tokenService;
			this.context = context;
		}

		[HttpPost("register")]
		public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
		{
			if (await UserExists(registerDto.UserName)) { return BadRequest("Username is taken"); }

			using var hmac = new HMACSHA512();

			var user = new AppUser
			{
				UserName = registerDto.UserName.ToLower(),
				PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
				PasswordSalt = hmac.Key
			};

			this.context.Users.Add(user);
			await this.context.SaveChangesAsync();

			return new UserDto
			{
				UserName = user.UserName,
				Token = this.tokenService.CreateToken(user)
			};
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
		{
			var user = await this.context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName.ToLower());

			if (user == null) { return Unauthorized("Invalid username"); }

			using var hmac = new HMACSHA512(user.PasswordSalt);

			var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

			for (int i = 0; i < computeHash.Length; i++)
			{
				if (computeHash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid Password"); }
			}

			return new UserDto
			{
				UserName = user.UserName,
				Token = this.tokenService.CreateToken(user)
			};
		}

		private async Task<bool> UserExists(string username)
		{
			return await this.context.Users.AnyAsync(u => u.UserName == username.ToLower());
		}

	}
}