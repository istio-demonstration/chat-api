using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Like;
using API.Entities;

namespace API.Interfaces
{
  public  interface ILikeRepository
  {
      Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
      Task<AppUser> GetUserWithLikes(int userId);
      Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId);
  }
}
