using Microsoft.EntityFrameworkCore;

namespace WebApplication6.Models
{
    public class DB : DbContext
    {
        private readonly IConfiguration _config;
        public DB(IConfiguration config) 
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config["ConnectionStrings"];
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<logins> logins { get; set; }
    }
}
