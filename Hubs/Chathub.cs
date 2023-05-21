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

        public async Task SendMessage(int loggedInUser, string messageContent, string chatRoomName)
        {
            Account sender = await _db.Accounts.Where(a => a.Id == loggedInUser).SingleAsync();
            ChatRoom chatRoom = await _db.ChatRooms.Where(room => room.Name == chatRoomName).SingleAsync();

            Message message = new()
            {
                Content = messageContent,
                Sender = sender,
                TimeStamp = DateTime.UtcNow,
                ChatRoom = chatRoom
            };

            // Save message to db
            await _db.Messages.AddAsync(message);
            await _db.SaveChangesAsync();

            await Clients.Group(chatRoom.Name).SendAsync("ReceiveMessage", message);
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

        public async Task LoadPreviousMessages(string chatRoom)
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

        public async Task CreateChatRoom(int loggedInUser, string chatRoomName)
        {
            Account owner = await _db.Accounts.Where(a => a.Id == loggedInUser).SingleAsync();

            ChatRoom chatRoom = new()
            {
                Name = chatRoomName,
                Owner = owner
            };

            await _db.ChatRooms.AddAsync(chatRoom);
            await _db.SaveChangesAsync();

            await Clients.Caller.SendAsync("CreateChatRoom", chatRoom);
        }

        public async Task JoinChatRoom(int loggedInUser, string chatRoomName)
        {
            ChatRoom chatRoom = await _db.ChatRooms.Where(room => room.Name == chatRoomName).SingleAsync();
            Account user = await _db.Accounts.Where(a => a.Id == loggedInUser).SingleAsync();

            chatRoom.Users.Add(user);

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomName);
        }

        public async Task LeaveChatRoom(string chatRoomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomName);
        }
    }
}
