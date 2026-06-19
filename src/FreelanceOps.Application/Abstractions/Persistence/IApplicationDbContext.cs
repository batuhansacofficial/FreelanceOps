using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Clients;
using FreelanceOps.Domain.Projects;
using FreelanceOps.Domain.TimeTracking;
using FreelanceOps.Domain.Users;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Workspace> Workspaces { get; }

    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    DbSet<Client> Clients { get; }

    DbSet<Project> Projects { get; }

    DbSet<ProjectTask> ProjectTasks { get; }

    DbSet<TimeEntry> TimeEntries { get; }

    DbSet<Invoice> Invoices { get; }

    DbSet<InvoiceItem> InvoiceItems { get; }

    DbSet<PaymentRecord> PaymentRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
