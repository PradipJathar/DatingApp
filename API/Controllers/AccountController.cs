using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AccountController(DataContext dataContext, ITokenService tokenService, IMapper mapper)
        {
            db = dataContext;
            _tokenService = tokenService;
            _mapper = mapper;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }

            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Username.ToLower();

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
        {
            var user = await db.Users.Include(m => m.Photos)
                                     .SingleOrDefaultAsync(m => m.UserName == loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(m => m.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        

        private async Task<bool> UserExists(string username)
        {
            return await db.Users.AnyAsync(m => m.UserName == username.ToLower());
        }
    }
}
