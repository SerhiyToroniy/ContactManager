using Microsoft.EntityFrameworkCore;

namespace Contact_Manager.Models
{
    public class AppDbContext : DbContext
    {

        public DbSet<CsvDataModel> CsvDataModels { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}

