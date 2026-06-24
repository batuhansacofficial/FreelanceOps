namespace FreelanceOps.Infrastructure.Persistence.Seeding;

public interface IDemoDataSeeder
{
    Task<DemoSeedResult> SeedAsync(
        bool resetBeforeSeed = false,
        CancellationToken cancellationToken = default);
}
