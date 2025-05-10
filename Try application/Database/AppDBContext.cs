using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Try_application.Database.Entities;

namespace Try_application.Database
{
    public class AppDBContext : IdentityDbContext<User>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }  // ✅ Entity for Product table
        public DbSet<Banner> Banners { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }// ✅ New Bookmark DbSet
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Review> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CartItem config
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .Property(c => c.UserId)
                .IsRequired(false);

            modelBuilder.Entity<CartItem>()
                .Property(c => c.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(c => c.DateAdded)
                .HasDefaultValueSql("NOW()");

            // ✅ Bookmark config
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Book)
                .WithMany()
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bookmark>()
                .Property(b => b.UserId)
                .IsRequired(false);

            modelBuilder.Entity<Bookmark>()
                .Property(b => b.DateAdded)
                .HasDefaultValueSql("NOW()");
        }

        // Optional: Ensure CartItems table (legacy support if you want to keep this)
        public bool EnsureCartItemsTableExists()
        {
            try
            {
                var sql = @"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'CartItems') THEN
                        CREATE TABLE ""CartItems"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""UserId"" TEXT NULL,
                            ""ProductId"" INTEGER NOT NULL,
                            ""Quantity"" INTEGER NOT NULL,
                            ""UnitPrice"" DECIMAL(18,2) NOT NULL,
                            ""DateAdded"" TIMESTAMP NOT NULL DEFAULT NOW(),
                            CONSTRAINT ""FK_CartItems_Products_ProductId"" FOREIGN KEY (""ProductId"") 
                                REFERENCES ""Products"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX ""IX_CartItems_ProductId"" ON ""CartItems"" (""ProductId"");
                    END IF;
                END
                $$;";
                this.Database.ExecuteSqlRaw(sql);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating CartItems table: {ex.Message}");
                return false;
            }
        }
    }
}
