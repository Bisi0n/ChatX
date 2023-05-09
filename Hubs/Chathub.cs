using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private static int _messageId = 0;
        private readonly AppDbContext database;

        public Chathub (AppDbContext context)
        {
            database = context;
        }

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

            // Save message to db
            History messageHistory = new()
            {
                MessageId = Interlocked.Increment(ref _messageId),
                User = loggedInUserName,
                Content = messageContent,
                TimeStamp = DateTime.UtcNow
            };
            database.Historys.Add(messageHistory);
            await database.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int id)
        {
            // Delete from db here
            // Counsult with Customer if to keep/delete the message history
            
            await Clients.All.SendAsync("deleteMessageRemote", id);
        }
    }
}
