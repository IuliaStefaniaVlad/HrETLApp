using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrappRepositories.DBContext
{
    public class TenantDbContext : IdentityDbContext<IdentityUser>
    {


        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IdentityUser>();
            base.OnModelCreating(builder);
        }

    }
}
