using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs.Message;
using API.Entities;
using API.Helper;

namespace API.Interfaces
{
  public  interface IMessageRepository
  {
      void AddGroup(Group group);
      void RemoveConnection(Connection connection);
      Task<Connection> GetConnection(string connectionId);

      Task<Group> GetMessageGroup(string groupName);
      Task<Group> GetGroupForConnection(string connectionId);

      void AddMessage(Message message);
      void DeleteMessage(Message message);

      Task<Message> GetMessage(int id);

      Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
        // get conversation of  users
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername);

      Task<bool> SaveAllAsync();
  }
}
