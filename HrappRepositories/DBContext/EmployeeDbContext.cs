using HrappModels;
using Microsoft.EntityFrameworkCore;

namespace HrappRepositories.DBContext
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
        { }

        public DbSet<EmployeeDBModel> Employee { get; set; }
        
    }
}