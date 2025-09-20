using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        // 
        // public string DbPath { get; }

        // public DataContext()
        // {
        //     var folder = Environment.SpecialFolder.LocalApplicationData;
        //     var path = Environment.GetFolderPath(folder);
        //     DbPath = System.IO.Path.Join(path, "Blogbox.db");
        // }

        // 
        // protected override void OnConfiguring(DbContextOptionsBuilder options)
        // {
        //     options.UseSqlite($"Data Source={DbPath}");
        // }
    }
}
