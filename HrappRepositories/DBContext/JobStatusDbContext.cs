using HrappModels;
using Microsoft.EntityFrameworkCore;

namespace HrappRepositories.DBContext
{
    public class JobStatusDbContext : DbContext
    {
        public JobStatusDbContext(DbContextOptions<JobStatusDbContext> options)
        : base(options)
        { }

        public DbSet<JobStatusModel> JobStatus { get; set; }
    }
}
