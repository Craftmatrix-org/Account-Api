using Microsoft.EntityFrameworkCore;
using Craftmatrix.org.ConnString;

namespace Craftmatrix.org.DB
{
    public class UserContext() : DbContext
    {
        ConnStrings connString;

        public UserContext(DbContextOptions<UserContext> options) : this()
        {
            connString = new ConnStrings();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connString.PostGres());
            }
        }
        public DbSet<UserDto> User { get; set; }
        //public DbSet<Whoami> WhoAmI { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
