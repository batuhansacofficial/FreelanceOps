using FreelanceOps.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("freelanceops_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly Dictionary<string, string?> _originalEnvironmentValues = [];

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        SetTestEnvironmentVariable("ConnectionStrings__Database", _postgres.GetConnectionString());
        SetTestEnvironmentVariable("Jwt__Issuer", "FreelanceOps.Tests");
        SetTestEnvironmentVariable("Jwt__Audience", "FreelanceOps.Api.Tests");
        SetTestEnvironmentVariable("Jwt__Secret", "integration-tests-secret-key-minimum-32-characters");
        SetTestEnvironmentVariable("Jwt__AccessTokenExpirationMinutes", "15");
        SetTestEnvironmentVariable("Jwt__RefreshTokenExpirationDays", "7");

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        foreach (var (key, value) in _originalEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    private void SetTestEnvironmentVariable(string key, string value)
    {
        _originalEnvironmentValues.TryAdd(key, Environment.GetEnvironmentVariable(key));
        Environment.SetEnvironmentVariable(key, value);
    }
}
