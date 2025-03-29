using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.API
{
    public sealed class DataContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Variation> Variations { get; set; } // added to test variations on system

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedData.Load(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
    }
}
