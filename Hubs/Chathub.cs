using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private readonly AppDbContext _db;
        private Dictionary<string, List<Account>> _usersCurrentlyTyping;
        private readonly Account _systemAccount;

        public Chathub(AppDbContext context)
        {
            _db = context;
            _usersCurrentlyTyping = new();
            _systemAccount = new()
            {
                Id = -1,
                Name = "System"
            };
        }

        public async Task SendMessage(int loggedInUser, string messageContent, int roomId)
        {
            Account sender = await GetUserAsync(loggedInUser);
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);
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

            await Clients.Group(chatRoom.GetRoomIdentifier()).SendAsync("ReceiveMessage", message);
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

        public async Task LoadPreviousMessages(int roomId)
        {
            Message[] messages = await _db.Messages.Include(m => m.Sender)
                .Where(m => !m.IsDeleted && m.ChatRoom.Id == roomId).ToArrayAsync();

            await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
        }

        public async Task UserTyping(int loggedInUser, bool isTyping, int roomId)
        {
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);
            Account user = await GetUserAsync(loggedInUser);

            string roomIdentifier = chatRoom.GetRoomIdentifier();

            if (!_usersCurrentlyTyping.ContainsKey(roomIdentifier))
            {
                _usersCurrentlyTyping[roomIdentifier] = new();
            }

            List<Account> usersTyping = _usersCurrentlyTyping[roomIdentifier];

            if (isTyping)
            {
                if (!usersTyping.Contains(user))
                {
                    usersTyping.Add(user);
                }
            }
            else
            {
                usersTyping.Remove(user);
            }

             await Clients.Group(chatRoom.GetRoomIdentifier()).SendAsync("CurrentlyTyping", usersTyping);
        }

		public async Task LoadChatRooms()
		{
            ChatRoom[] rooms = await _db.ChatRooms.Include(r => r.CreatedBy).Where(r => !r.IsDeleted).ToArrayAsync();

			await Clients.Caller.SendAsync("ReceiveChatRooms", rooms);
		}

		public async Task CreateChatRoom(int loggedInUser, string chatRoomName)
        {
            Account createdBy = await GetUserAsync(loggedInUser);

            ChatRoom chatRoom = new()
            {
                Name = chatRoomName,
                CreatedBy = createdBy
            };

            await _db.ChatRooms.AddAsync(chatRoom);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("CreateChatRoom", chatRoom);
        }

        public async Task DeleteChatRoom(int loggedInUser, int id)
        {
            ChatRoom chatRoom = await GetChatRoomAsync(id);

            if (chatRoom.CreatedBy.Id != loggedInUser)
            {
                return;
            }

            chatRoom.IsDeleted = true;

            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("DeleteChatRoom", id);
        }

        public async Task JoinChatRoom(int roomId)
        {
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoom.GetRoomIdentifier());
        }

        public async Task LeaveChatRoom(int roomId)
        {
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoom.GetRoomIdentifier());
        }

        public async Task AnnounceActivity(int loggedInUser, int roomId, bool joined)
        {
            Account user = await GetUserAsync(loggedInUser);
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);

            string content = joined ? $"{user.Name} joined {chatRoom.Name}!" : $"{user.Name} left {chatRoom.Name}!";

            await Clients.Group(chatRoom.GetRoomIdentifier()).SendAsync("ReceiveMessage", new Message()
            {
                Sender = _systemAccount,
                Content = content,
                TimeStamp = DateTime.UtcNow,
                ChatRoom = chatRoom
            });
        }

        private async Task<ChatRoom> GetChatRoomAsync(int roomId)
        {
            ChatRoom chatRoom = await _db.ChatRooms.Include(r => r.CreatedBy).Where(r => r.Id == roomId).SingleAsync();

            return chatRoom;
        }

        private async Task<Account> GetUserAsync(int userId)
        {
            Account user = await _db.Accounts.Where(a => a.Id == userId).SingleAsync();

            return user;
        }
    }
}