using System;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using API.DTOs.Message;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;

        public MessageHub(IUnitOfWork unitOfWork, IMapper mapper, 
                          IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var callerUserName = Context.User.GetUsername();
            var otherUserUserName = httpContext.Request.Query["user"].ToString();
           
            var groupName = GetGroupName(callerUserName, otherUserUserName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group =  await AddToGroup(groupName);
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            var messages = await _unitOfWork.MessageRepository.GetMessageThread(callerUserName, otherUserUserName);
            //await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();
            
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // signalR will remove her/him from group automatically when anyone disconnected
          var group =  await RemoveFromMessageGroup();
          await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            // check username Equals to current user's username
            if (username == createMessageDto.RecipientUsername) throw new HubException("You cannot send messages to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByIdAsync(Context.User.GetUserId());

            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");

            var message = new Message()
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {  // the user that you are talking to currently is listening on topic "NewMessage"
                message.DateRead = DateTimeOffset.Now;
            }
            else 
            {
                // the user that you are talking to currently is not  listening on topic "NewMessage"
                var connections = await _presenceTracker.GetConnectionForUser(recipient.UserName);
                if (connections != null) // recipient is online
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }
            if (await _unitOfWork.Complete())
            {
                var messageDto = _mapper.Map<MessageDto>(message);
                await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
            };

        }

        private static string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }


        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete()) return group;
           
            throw new HubException("failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await _unitOfWork.Complete()) return group;
           throw new HubException("failed to remove from group");

            
        }
    }
}
