using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Controllers
{
    [Route("api")]
    [ApiController]
    [AllowAnonymous]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext database)
        {
            _db = database;
        }

        [HttpGet]
        [Authorize(Policy = "AllowAnonymous")]
        public ActionResult<List<Message>> GetLatestMessages()
        {
            var lastFive = _db.Messages.Include(m => m.Sender)
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Id)
                .Take(5);

            if (!lastFive.Any())
            {
                return NotFound();
            }

            return lastFive.ToList();
        }
    }
}