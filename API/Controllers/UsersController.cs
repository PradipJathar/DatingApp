using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        public readonly IPhotoService _photoService;

        public UsersController(IUserRepository useRepo, IMapper mapper, IPhotoService photoService)
        {
            _userRepo = useRepo;
            _mapper = mapper;
            _photoService = photoService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            var users = await _userRepo.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }


        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepo.GetMemberAsync(username);
        }


        [HttpPut]
        public async Task<ActionResult> UpdateUser([FromBody] MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user);

            _userRepo.Update(user);

            if (await _userRepo.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to update user.");
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _userRepo.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { userName = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(m => m.Id == photoId);
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo.");
            }

            var currentMainPhoto = user.Photos.FirstOrDefault(m => m.IsMain);
            if (currentMainPhoto != null)
            {
                currentMainPhoto.IsMain = false;
            }

            photo.IsMain = true;

            if (await _userRepo.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to set main photo.");
        }


        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(m => m.Id == photoId);

            if (photo == null)
            {
                return NotFound();
            }

            if (photo.IsMain)
            {
                return BadRequest("You can not delete your main photo.");
            }

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if (await _userRepo.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to delete the photo.");
        }
    }
}
