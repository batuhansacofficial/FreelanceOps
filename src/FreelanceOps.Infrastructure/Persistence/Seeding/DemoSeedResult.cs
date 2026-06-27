namespace FreelanceOps.Infrastructure.Persistence.Seeding;

public sealed record DemoSeedResult(
    Guid UserId,
    Guid WorkspaceId,
    bool Created);
