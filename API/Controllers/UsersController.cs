using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs.Member;
using API.DTOs.Photo;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class UsersController: BaseApiController
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly MapsterMapper.IMapper _mapster;
        private readonly IPhotoService _photoService;


        public UsersController(IUnitOfWork unitOfWork, MapsterMapper.IMapper mapster,
                               IPhotoService photoService)
        {

            _unitOfWork = unitOfWork;
            _mapster = mapster;
            _photoService = photoService;
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<MemberToReturnDto>>> GetUsers([FromQuery] MemberFilter request)
        {
            var currentUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            request.CurrentUsername = currentUser.UserName;
            if (string.IsNullOrEmpty(request.Gender))
            {
                request.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetMembersAsync(request);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPage);
            return Ok(users);
        }

        [HttpGet("GetUser/{username}",Name = "GetUser")]
        public async Task<ActionResult<MemberToReturnDto>> GetUser(string username)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            return _mapster.Map<MemberToReturnDto>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto model)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            _mapster.Map(model, user);
            _unitOfWork.UserRepository.Update(user);
            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("Fail to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo()
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (!user.Photos.Any())
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);
            if ( await _unitOfWork.Complete())
            {
                //return _mapster.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser",new {username = user.UserName }, _mapster.Map<PhotoDto>(photo));
            }

            return BadRequest("An error occurred during adding photo(s)");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return BadRequest("This photo is not exist");
            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMainPhoto != null)
            {
                currentMainPhoto.IsMain = !currentMainPhoto.IsMain;
            }

            photo.IsMain = true;
            if ( await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("an error occurred during setting main photo");


        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)

        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You can not delete your main photo");
            if (photo.PublicId == null)
            {
               var result =  await _photoService.DeletePhotoAsync(photo.PublicId);

               if (result.Error != null) return BadRequest(result.Error.Message);

            }

            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("an error occurred during deleting photo");
        }
    }
}
