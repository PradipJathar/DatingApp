using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext db;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext dataContext, ITokenService tokenService)
        {
            db = dataContext;
            _tokenService = tokenService;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                Username = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user)
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
        {
            var user = await db.Users.Include(m => m.Photos)
                                     .SingleOrDefaultAsync(m => m.Username == loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(m => m.IsMain)?.Url
            };
        }
        

        private async Task<bool> UserExists(string username)
        {
            return await db.Users.AnyAsync(m => m.Username == username.ToLower());
        }
    }
}
