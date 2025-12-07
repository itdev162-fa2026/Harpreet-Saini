using Microsoft.EntityFrameworkCore;
using Domain;

namespace Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        
        // No OnConfiguring needed because connection comes from appsettings.json
    }
}
