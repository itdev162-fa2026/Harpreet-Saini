using Microsoft.EntityFrameworkCore;
using Domain; // Make sure you have this to reference Product class

namespace Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Add this line to include your Product entity in EF Core context
        public DbSet<Product> Products { get; set; }
    }
}
