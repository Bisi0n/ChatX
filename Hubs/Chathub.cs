using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private readonly AppDbContext database;

        public Chathub (AppDbContext historyDb)
        {
            database = historyDb;
        }
        public async Task SendMessage(string user, string message)
        {
            var messageHistory = new History
            {
                User = user,
                Message = message,
                Date = DateTime.Now
            };

            database.Historys.Add(messageHistory);
            await database.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task AddEmoji(string user, string message, string emoji)
        {
            await Clients.All.SendAsync($"ReceiveReaction", user, message, emoji);
        }
    }
}
