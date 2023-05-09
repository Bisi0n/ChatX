using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private static int _messageId = 0;

        public async Task SendMessage(string loggedInUserName, int loggedInUser, string messageContent)
        {
            Message message = new()
            {
                Id = Interlocked.Increment(ref _messageId),
                Content = messageContent,
                Sender = loggedInUser,
                SenderName = loggedInUserName,
                TimeStamp = DateTime.UtcNow
            };

            // If we want to store messages, add them here to a db

            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int id)
        {
            // Delete from db here

            await Clients.All.SendAsync("deleteMessageRemote", id);
        }
    }
}
