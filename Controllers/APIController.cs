using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Controllers
{
    [Route("/api")]
    [AllowAnonymous]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly AppDbContext _db;

        public APIController(AppDbContext database)
        {
            _db = database;
        }

        // Get latest messages
        [HttpGet("latest/{amount:int?}")]
        public ActionResult<List<Message>> GetLatestMessages(int? amount = null)
        {
            // If no amount is passed in, return 5 latest messages.
            int messageCount = amount ?? 5;

            var lastMessages = _db.Messages.Include(m => m.Sender)
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Id)
                .Take(messageCount);

            if (!lastMessages.Any())
            {
                return NotFound();
            }

            return lastMessages.ToList();
        }
    }
}
