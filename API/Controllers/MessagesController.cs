using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs.Message;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unit, IMapper mapper)
        {
            _unit = unit;
            _mapper = mapper;
        }

        [HttpPost]

        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
           
            // check username Equals to current user's username
            if (username == createMessageDto.RecipientUsername) return BadRequest("You can't send messages to yourself");

            var sender = await _unit.UserRepository.GetUserByIdAsync(User.GetUserId());

            var recipient = await _unit.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();
          
            var message = new Message()
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unit.MessageRepository.AddMessage(message);

            if (await _unit.Complete()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("Fail to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
            [FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _unit.MessageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPage);

            return messages;
        }

        ///// <summary>
        ///// the function was implement in signalR
        ///// </summary>
        ///// <param name="username">username is other user that current login user talking to</param>
        ///// <returns></returns>
        //[HttpGet("thread/{username}")]
        //public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        //{
        //    var currentUsername = User.GetUsername();

        //    return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        //}

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _unit.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();

            if (message.Sender.UserName == username) 
            {
                message.SenderDeleted    = true;
            }

            if (message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _unit.MessageRepository.DeleteMessage(message);
            }

            if (await _unit.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
