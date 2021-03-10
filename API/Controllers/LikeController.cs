using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs.Like;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikeController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IUserRepository _userRepository;
        //private readonly ILikeRepository _unitOfWork.LikeRepository;

        public LikeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            //_userRepository = userRepository;
            //_unitOfWork.LikeRepository = likeRepository;
        }

        /// <summary>
        ///  like someone 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            var sourceUser = await _unitOfWork.LikeRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You are not allowed to like yourself");

            var userLike = await _unitOfWork.LikeRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("You already like this user");
            
            userLike = new UserLike()
            {
                SourceUserId = sourceUserId,
                LikeUserId = likedUser.Id,
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
        {
            return Ok(await _unitOfWork.LikeRepository.GetUserLikes(predicate, User.GetUserId()));
        }
    }
}
