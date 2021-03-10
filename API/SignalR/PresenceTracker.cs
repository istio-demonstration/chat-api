using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        // key is gonna be user's username, every time user connect to the hub,
        // they will be given a connectionId in a string form ,
        // there is nothing to stop a user from connecting to the same application from a different device,
        // and they would get a different connectionId for each different connection that they're having or making
        // to our application.so value will store a list of connectionId
#pragma warning disable IDE1006 // 命名样式
        private static readonly Dictionary<string, List<string>> OnlineUsers =
#pragma warning restore IDE1006 // 命名样式
            new Dictionary<string, List<string>>();

        /// <summary>
        /// the Dictionary (OnlineUsers) is gonna be shared among everyone who connect to
        /// our server and Dictionary is not a thread safe resource, so if we had concurrent users trying to
        /// update this at the same time , then we're probably going to run into problems.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Task<bool> UserConnected(string username, string connectionId)
        {
            var isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string>(){ connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            var isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(false);

                OnlineUsers[username].Remove(connectionId);
                if (OnlineUsers[username].Count ==0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key)
                    .Select(x => x.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }
    }

   
}
