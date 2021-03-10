using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.GetUsername();
            var isOnline = await _tracker.UserConnected(username, Context.ConnectionId);
            if (isOnline)
                await Clients.Others.SendAsync("UserIsOnline", username);
            //return base.OnConnectedAsync();
            var onlineUsers = await _tracker.GetOnlineUsers();
            //await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);
            await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsers);

        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            //var onlineUsers = await _tracker.GetOnlineUsers();
            //await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
