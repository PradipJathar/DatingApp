using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext db;

        public AccountController(DataContext dataContext)
        {
            db = dataContext;
        }


        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswardHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return user;
        }


        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDto loginDto) 
        {
            var user = await db.Users.SingleOrDefaultAsync(m => m.UserName == loginDto.UserName);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswardHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return user;
        }
        

        private async Task<bool> UserExists(string username)
        {
            return await db.Users.AnyAsync(m => m.UserName == username.ToLower());
        }
    }
}
