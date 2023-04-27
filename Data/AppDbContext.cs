using Chatx.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Chatx.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
