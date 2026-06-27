using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Clients;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Projects;
using FreelanceOps.Domain.Proposals;
using FreelanceOps.Domain.TimeTracking;
using FreelanceOps.Domain.Users;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

    public DbSet<Proposal> Proposals => Set<Proposal>();

    public DbSet<ProposalItem> ProposalItems => Set<ProposalItem>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("freelance_ops");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
