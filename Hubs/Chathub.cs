using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System;
using System.IO;

namespace ChatX.Hubs
{
    public class Chathub
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public Chathub(AppDbContext context)
        {
            _db = context;
        }
        public Chathub(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task SendMessage(string loggedInUserName, int loggedInUser, string messageContent)
        {
            Message message = new()
            {
                Content = messageContent,
                Sender = loggedInUser,
                SenderName = loggedInUserName,
                TimeStamp = DateTime.UtcNow,
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

            await Clients.All.SendAsync("DeleteMessage", id);
        }

        public async Task LoadPreviousMessages()
        {
            Message[] messages = await _db.Messages.ToArrayAsync();

            await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
        }

        public async Task AddReaction(string reaction)
        {
            await Clients.All.SendAsync("RecieveReaction", reaction);
        }

        public async Task UploadImage(byte[] data, string contentType)
        {
            // Give unique path so img dont get overwritten
            var fileName = Guid.NewGuid().ToString() + GetFileType(contentType);

            // We choose root directory to not allow user to specify arbitrary file path
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            // Tempory filePath on "client side" to store img
            var filePath = Path.Combine(uploadsRoot, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await stream.WriteAsync(data, 0, data.Length);

            var imageUrl = $"{Context.GetHttpContext().Request.Scheme}://{Context.GetHttpContext().Request.Host}/uploads/{fileName}";

            Image newImage = new()
            {
                FileName = fileName,
                ContentType = contentType,
                Data = data,
                Url = imageUrl
            };

            _db.Images.Add(newImage);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("RecieveImage", imageUrl);

        }
        private static string GetFileType(string contentType)
        {
            switch (contentType)
            {
                case "image/jpeg":
                    return ".jpg";
                case "image/png":
                    return ".png";
                case "image/gif":
                    return ".gif";
                // ask customer for other content such as mp3/mp4 etc
                default:
                    throw new ArgumentException($"Unsupported file type: {contentType}");
            }
        }
    }
}