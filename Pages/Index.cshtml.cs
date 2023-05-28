using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace ChatX.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        public List<ChatRoom> ChatRooms { get; set; }

        public IndexModel(AppDbContext database)
        {
            _db = database;
            ChatRooms = _db.ChatRooms
                .Include(r => r.CreatedBy).ToList();
        }

        public void OnGet()
        {

        }
    }
}