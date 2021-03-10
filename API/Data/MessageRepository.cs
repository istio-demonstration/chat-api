using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Message;
using API.Entities;
using API.Helper;
using API.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }


        /// <inheritdoc />
        public void AddGroup(Group @group)
        {
            _context.Groups.Add(group);
        }

        /// <inheritdoc />
        public void RemoveConnection(Connection connection)
        {
            _context.Remove(connection);
        }

        /// <inheritdoc />
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        /// <inheritdoc />
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        /// <inheritdoc />
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(x => x.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public void AddMessage(Message message)
        {
            _context.Message.Add(message);
        }

        /// <inheritdoc />
        public void DeleteMessage(Message message)
        {
            _context.Message.Remove(message);
        }

        /// <inheritdoc />
        public async Task<Message> GetMessage(int id)
        {
            // FindAsync wont include Sender and Recipient
            return await _context.Message
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        /// <inheritdoc />
        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Message
                .OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false ),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false ),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && u.DateRead == null && u.RecipientDeleted == false)
            };

            var messages = query.ProjectToType<MessageDto>(_mapper.Config);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Message
                //.Include(u => u.Sender).ThenInclude(p => p.Photos)
                //.Include(r => r.Recipient).ThenInclude(r=>r.Photos) cuz of ProjectToType, wo don' need Include any more at the query
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false && m.Sender.UserName == recipientUsername || m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && m.SenderDeleted == false)
                .OrderBy(x => x.MessageSent)
                .ProjectToType<MessageDto>(_mapper.Config)
                .ToListAsync();
            // mark message to read because current user was reading
            var unreadMessages = messages.Where(m => m.DateRead == null
                                                     && m.RecipientUsername == currentUsername).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTimeOffset.Now;
                }

                //Parallel.ForEach(unreadMessages,
                //    new ParallelOptions() {MaxDegreeOfParallelism = System.Environment.ProcessorCount}, (message) =>
                //    {
                //        message.DateRead = DateTimeOffset.Now;
                //    });

                //await _context.SaveChangesAsync(); not the repository's job
            }

            return messages;
        }

        /// <inheritdoc />
        public async Task<bool> SaveAllAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
