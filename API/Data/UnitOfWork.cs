using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using MapsterMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        /// <inheritdoc />
        public IUserRepository UserRepository  => new UserRepository(_context, _mapper);

        /// <inheritdoc />
        public IMessageRepository MessageRepository => new MessageRepository(_mapper, _context);

        /// <inheritdoc />
        public ILikeRepository LikeRepository => new LikeRepository(_context);

        /// <inheritdoc />
        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
