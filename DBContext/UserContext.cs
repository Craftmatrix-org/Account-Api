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
        DbSet<UserDto> User { get; set; }
    }
}
