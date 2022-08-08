using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class UsersController : BaseApiController
	{
		private readonly DataContext context;
		public UsersController(DataContext context)
		{
			this.context = context;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
		{
			var users = await this.context.Users.ToListAsync();

			return users;
		}

		[Authorize]
		// api/users/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<AppUser>> GetUser(int id)
		{
			return await this.context.Users.FindAsync(id);
		}
	}
}