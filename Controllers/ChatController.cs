using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Controllers
{
    [Route("/api")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext database)
        {
            _db = database;
        }

        [HttpGet()]
        public ActionResult<List<Message>> GetLatestMessages()
        {
            var lastFive = _db.Messages.OrderByDescending(m => m.Id).Take(5);

            if (lastFive == null || !lastFive.Any())
            {
                return NotFound();
            }
            return lastFive.ToList();
        }
    }
}