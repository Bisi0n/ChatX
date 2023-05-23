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

        // Get last five messages
        [HttpGet]
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
