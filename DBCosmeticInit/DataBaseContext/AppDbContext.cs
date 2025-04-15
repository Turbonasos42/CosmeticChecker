using DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace DatabaseContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Настройки модели
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasKey(d => d.Id); // Указываем, что Id является ключом
        }
    }
}