using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Controllers
{
    [Route("/chatX")]
    [AllowAnonymous]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly AppDbContext database;

        public APIController(AppDbContext database)
        {
            this.database = database;
        }

        // Get last five messages
        [HttpGet("chatX/api")]
        public ActionResult<List<Message>> GetLatestMessages()
        {
            var allMessage = database.Messages.ToList();
            var lastFive = allMessage.Take(5);

            if(lastFive == null || !lastFive.Any())
            {
                return NotFound();
            }
            return lastFive.ToList();
        }
    }
}
