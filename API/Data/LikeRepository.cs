using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Like;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikeRepository : ILikeRepository
    {
        private readonly DataContext _context;

        public LikeRepository(DataContext context)
        {
            _context = context;
        }
        /// <inheritdoc />
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        /// <inheritdoc />
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(x => x.LikedUsers).FirstOrDefaultAsync(x => x.Id == userId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            //var users = _context.Users.OrderBy(x => x.Username).AsQueryable();
            IQueryable<AppUser> users;
            var likes = _context.Likes.AsQueryable();

            switch (predicate)
            {
                case "likedBy":
                    likes = likes.Where(l => l.LikeUserId == userId);
                    users = likes.Select(l => l.SourceUser);
                    break;
                default: // liked
                    likes = likes.Where(l => l.SourceUserId == userId);
                    users = likes.Select(l => l.LikedUser);
                    break;
            }

            //if (predicate == "liked")
            //{
            //    likes = likes.Where(l => l.SourceUserId == userId);
            //    users = likes.Select(l => l.LikedUser);
            //}

            //if (predicate == "likedBy")
            //{
            //    likes = likes.Where(l => l.LikeUserId == userId);
            //    users = likes.Select(l => l.SourceUser);
            //}

            return await users.Select(user => new LikeDto()
            {
                 Username = user.UserName,
                 KnownAs = user.KnownAs,
                 Age = user.DateOfBirth.CalculateAge(),
                 MainPhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                 City = user.City,
                 Id = user.Id
            }).ToListAsync();
        }
    }
}
