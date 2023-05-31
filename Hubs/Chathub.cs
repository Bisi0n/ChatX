using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace ChatX.Hubs
{
    public class Chathub : Hub
    {
        private readonly AppDbContext _db;
        private Dictionary<string, List<Account>> _usersCurrentlyTyping;
        private readonly Account _systemAccount;
        private readonly HttpClient _httpClient;
        private readonly string[] _interests = { "travel", "literature", "music", "sports", "film", "art", "cooking", "photography", "gardening", "programming" };

        public Chathub(AppDbContext context, HttpClient httpClient)
        {
            _db = context;
            _usersCurrentlyTyping = new();
            _systemAccount = new()
            {
                Id = -1,
                Name = "System"
            };
            _httpClient = httpClient;
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

            await _db.Messages.AddAsync(message);
            await _db.SaveChangesAsync();

            await Clients.Group(chatRoom.GetRoomIdentifier()).SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int id)
        {
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

        public async Task DeleteChatRoom(int loggedInUser, int roomId)
        {
            Message[] messages = await _db.Messages.Include(m => m.Sender)
                .Where(m => !m.IsDeleted && m.ChatRoom.Id == roomId).ToArrayAsync();

            foreach (var message in messages)
            {
                message.IsDeleted = true;
            }

            ChatRoom chatRoom = await GetChatRoomAsync(roomId);

            if (chatRoom.CreatedBy.Id != loggedInUser)
            {
                return;
            }

            chatRoom.IsDeleted = true;

            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("DeleteChatRoom", roomId);
            await Clients.All.SendAsync("SendMess");
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

        public async Task AnnounceDeletedChatRoom(int loggedInUser, int roomId)
        {
            Account user = await GetUserAsync(loggedInUser);
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);

            string content = $"{user.Name} has deleted the \"{chatRoom.Name}\" chat room. Please leave this room and join a new one!";

            await SendSystemMessage("AnnounceDeletedChatRoom", chatRoom, content);
        }

        public async Task AnnounceActivity(int loggedInUser, int roomId, bool joined)
        {
            Account user = await GetUserAsync(loggedInUser);
            ChatRoom chatRoom = await GetChatRoomAsync(roomId);

            string content = joined ? $"{user.Name} joined {chatRoom.Name}!" : $"{user.Name} left {chatRoom.Name}!";
            await SendSystemMessage("ReceiveMessage", chatRoom, content);
        }

        public async Task GetDateMatch(string input)
        {
            DateMatch? match = null;

            string? matchedInterest = _interests.FirstOrDefault(interest => input.ToLower().Contains(interest));

            if (!string.IsNullOrEmpty(matchedInterest))
            {
                string url = $"https://tinderapp.azurewebsites.net/users/{matchedInterest}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    match = JsonConvert.DeserializeObject<DateMatch>(apiResponse);

                }
            }

            await Clients.Caller.SendAsync("ReceiveDateMatch", match);
        }

        private async Task SendSystemMessage(string method, ChatRoom chatRoom, string content)
        {
            await Clients.Group(chatRoom.GetRoomIdentifier()).SendAsync(method, new Message()
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