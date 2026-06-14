using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("freelance_ops");

        base.OnModelCreating(modelBuilder);
    }
}
