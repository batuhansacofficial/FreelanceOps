using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreelanceOps.Infrastructure.Persistence.Seeding;

public sealed class DemoSeedHostedService(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<DemoSeedOptions> options,
    ILogger<DemoSeedHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var demoSeedOptions = options.Value;

        if (!environment.IsDevelopment() || !demoSeedOptions.Enabled)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDemoDataSeeder>();
        var result = await seeder.SeedAsync(
            demoSeedOptions.ResetBeforeSeed,
            cancellationToken);

        logger.LogInformation(
            "Demo seed completed for workspace {WorkspaceId}. Created new dataset: {Created}.",
            result.WorkspaceId,
            result.Created);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
