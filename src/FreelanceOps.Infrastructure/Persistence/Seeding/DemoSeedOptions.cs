namespace FreelanceOps.Infrastructure.Persistence.Seeding;

public sealed class DemoSeedOptions
{
    public const string SectionName = "DemoSeed";

    public bool Enabled { get; init; }

    public bool ResetBeforeSeed { get; init; }
}
