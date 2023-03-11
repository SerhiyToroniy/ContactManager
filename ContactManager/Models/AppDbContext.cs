using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace ContactManager.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }



        public DbSet<Person> People { get; set; }
    }

}
