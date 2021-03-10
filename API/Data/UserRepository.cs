using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Member;
using API.Entities;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository

    {
        private readonly DataContext _context;

        private readonly MapsterMapper.IMapper _mapster;
        //private readonly IMapper _autoMapper;

        public UserRepository(DataContext context, MapsterMapper.IMapper mapster)
        {
            _context = context;
    
            _mapster = mapster;
           // _autoMapper = autoMapper;
        }
        /// <inheritdoc />
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        /// <inheritdoc />
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Photos)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc />
        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
        }

        /// <inheritdoc />
        public async Task<PagedList<MemberToReturnDto>> GetMembersAsync(MemberFilter request)
        {
            var minDateOfBirth = DateTimeOffset.Now.AddYears(-request.MaxAge - 1);
            var maxDateOfBirth = DateTimeOffset.Now.AddYears(-request.MinAge);
            var query = _context.Users.Where(x => x.UserName != request.CurrentUsername
                                                  && x.Gender == request.Gender
           && x.DateOfBirth >= minDateOfBirth && x.DateOfBirth <= maxDateOfBirth);
            query = request.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
               
                _ => query.OrderByDescending(u => u.LastActive)
            };


            return await PagedList<MemberToReturnDto>
               .CreateAsync(query.ProjectToType<MemberToReturnDto>(_mapster.Config).AsNoTracking(),
               request.PageNumber, request.PageSize);

        }

        /// <inheritdoc />
        public async Task<MemberToReturnDto> GetMemberByUsernameAsync(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
                .ProjectToType<MemberToReturnDto>(_mapster.Config)
                .FirstOrDefaultAsync();
        }
    }
}
