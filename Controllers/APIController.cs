using ChatX.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatX.Controllers
{
    [Route("/api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly Data.AppDbContext database;

        public APIController(Data.AppDbContext database)
        {
            this.database = database;
        }

        [HttpGet]
        public string Get()
        {
            return "Example";
        }
    }
}
