using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private readonly AppDbContext _db;
        private List<Account> _usersCurrentlyTyping = new();

        public Chathub(AppDbContext context)
        {
            _db = context;
        }

        public async Task SendMessage(int loggedInUser, string messageContent)
        {
            Account sender = await _db.Accounts.Where(a => a.Id == loggedInUser).SingleAsync();

            Message message = new()
            {
                Content = messageContent,
                Sender = sender,
                TimeStamp = DateTime.UtcNow
            };

            // Save message to db
            await _db.Messages.AddAsync(message);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int id)
        {
            // Delete from db here
            // Counsult with Customer if to keep/delete the message history
            Message message = await _db.Messages.Where(m => m.Id == id).SingleAsync();
            message.IsDeleted = true;
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("DeleteMessage", id);
        }

        public async Task LoadPreviousMessages()
        {
            Message[] messages = await _db.Messages.Include(m => m.Sender)
                .Where(m => !m.IsDeleted).ToArrayAsync();

            await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
        }

        public async Task UserTyping(int loggedInUser, bool isTyping)
        {
            Account user = await _db.Accounts.Where(u => u.Id == loggedInUser).SingleAsync();

            if (isTyping)
            {
                _usersCurrentlyTyping.Add(user);
            }
            else
            {
                _usersCurrentlyTyping.Remove(user);
            }

            await Clients.All.SendAsync("CurrentlyTyping", _usersCurrentlyTyping);
        }

        public async Task AddEmojiReaction(int messageId, string emoji)
        {
            var message = await _db.Messages.Where(m => m.Id == messageId).SingleAsync();
            message.Reaction = emoji;
            _db.SaveChanges();

            await Clients.All.SendAsync("ReceiveEmojiReaction", messageId, emoji);
        }


    }
}