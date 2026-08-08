using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> User { get; set; }
    }
}