using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Billing;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Proposals;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.BackgroundJobs.ExpiredProposalJob;
using FreelanceOps.Application.BackgroundJobs.OverdueInvoiceNotificationJob;
using FreelanceOps.Infrastructure.Authentication;
using FreelanceOps.Infrastructure.BackgroundJobs;
using FreelanceOps.Infrastructure.Billing;
using FreelanceOps.Infrastructure.Notifications;
using FreelanceOps.Infrastructure.Persistence;
using FreelanceOps.Infrastructure.Persistence.Seeding;
using FreelanceOps.Infrastructure.Proposals;
using FreelanceOps.Infrastructure.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is missing.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IWorkspaceAccessService, WorkspaceAccessService>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddScoped<IProposalNumberGenerator, ProposalNumberGenerator>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IExpiredProposalJob, ExpiredProposalJob>();
        services.AddScoped<IOverdueInvoiceNotificationJob, OverdueInvoiceNotificationJob>();
        services.Configure<DemoSeedOptions>(configuration.GetSection(DemoSeedOptions.SectionName));
        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();
        services.AddHostedService<DemoSeedHostedService>();

        return services;
    }
}
