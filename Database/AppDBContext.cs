using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Try_application.Database.Entities;

namespace Try_application.Database
{
    public class AppDBContext : IdentityDbContext<User>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }  // ✅ Entity for Product table
        public DbSet<Book> Books { get; set; } = null!;
    }
}
