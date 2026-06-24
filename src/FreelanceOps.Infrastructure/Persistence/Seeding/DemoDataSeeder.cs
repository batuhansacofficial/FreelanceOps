using FreelanceOps.Application.Abstractions.Authentication;
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

namespace FreelanceOps.Infrastructure.Persistence.Seeding;

public sealed class DemoDataSeeder(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher) : IDemoDataSeeder
{
    public const string DemoEmail = "demo@freelanceops.dev";
    public const string DemoPassword = "Demo123!";
    public const string DemoWorkspaceSlug = "freelanceops-demo";

    public async Task<DemoSeedResult> SeedAsync(
        bool resetBeforeSeed = false,
        CancellationToken cancellationToken = default)
    {
        if (resetBeforeSeed)
        {
            await RemoveExistingDemoDataAsync(cancellationToken);
        }

        var normalizedEmail = User.NormalizeEmail(DemoEmail);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(
                existingUser => existingUser.Email == normalizedEmail,
                cancellationToken);

        var userCreated = false;

        if (user is null)
        {
            user = new User(
                DemoEmail,
                passwordHasher.Hash(DemoPassword),
                "Demo Freelancer");
            dbContext.Users.Add(user);
            userCreated = true;
        }

        var existingWorkspace = await dbContext.Workspaces
            .SingleOrDefaultAsync(
                workspace => workspace.Slug == DemoWorkspaceSlug,
                cancellationToken);

        if (existingWorkspace is not null)
        {
            if (userCreated)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new DemoSeedResult(user.Id, existingWorkspace.Id, Created: false);
        }

        var workspace = new Workspace(
            "FreelanceOps Demo Workspace",
            DemoWorkspaceSlug,
            user.Id);

        AddDemoDataset(workspace.Id, user.Id);
        dbContext.Workspaces.Add(workspace);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DemoSeedResult(user.Id, workspace.Id, Created: true);
    }

    private void AddDemoDataset(Guid workspaceId, Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var reportDate = CreateReportDate();

        var acmeClient = new Client(
            workspaceId,
            "Acme Studio",
            "ops@acmestudio.dev",
            "Acme Studio",
            "Primary demo client for backend API and retainer work.");
        var northwindClient = new Client(
            workspaceId,
            "Northwind Digital",
            "hello@northwinddigital.dev",
            "Northwind Digital",
            "Demo client for dashboard and reporting work.");

        var apiProject = new Project(
            workspaceId,
            acmeClient.Id,
            "SaaS Backend API",
            "Workspace-scoped API build with auth, billing, reports, and notifications.",
            today.AddDays(-45),
            today.AddDays(20));
        apiProject.ChangeStatus(ProjectStatus.Active);

        var retainerProject = new Project(
            workspaceId,
            acmeClient.Id,
            "Automation Retainer",
            "Accepted proposal converted into a recurring automation retainer.",
            today.AddDays(-10),
            today.AddDays(45));
        retainerProject.ChangeStatus(ProjectStatus.Active);

        var dashboardProject = new Project(
            workspaceId,
            northwindClient.Id,
            "Internal Dashboard",
            "Operational dashboard with revenue and project performance reporting.",
            today.AddDays(-35),
            today.AddDays(-2));
        dashboardProject.ChangeStatus(ProjectStatus.Completed);

        var tasks = CreateDemoTasks(
            workspaceId,
            userId,
            apiProject.Id,
            retainerProject.Id,
            dashboardProject.Id,
            today);

        var timeEntries = CreateDemoTimeEntries(
            workspaceId,
            userId,
            apiProject.Id,
            retainerProject.Id,
            dashboardProject.Id,
            tasks,
            reportDate);

        var acmeInvoice = new Invoice(
            workspaceId,
            acmeClient.Id,
            apiProject.Id,
            "DEMO-2026-001",
            today.AddDays(-12),
            today.AddDays(-2),
            "USD",
            "Partially paid demo invoice with an overdue balance.");
        acmeInvoice.AddItem("API implementation", 40m, 120m, 0m);
        acmeInvoice.AddItem("Architecture review", 2m, 150m, 0m);
        acmeInvoice.MarkAsSent();
        acmeInvoice.RecordPayment(
            2500m,
            PaymentMethod.BankTransfer,
            "DEMO-PAY-ACME-001",
            today.AddDays(-1));

        var northwindInvoice = new Invoice(
            workspaceId,
            northwindClient.Id,
            dashboardProject.Id,
            "DEMO-2026-002",
            today.AddDays(-8),
            today.AddDays(14),
            "USD",
            "Paid demo invoice for dashboard delivery.");
        northwindInvoice.AddItem("Dashboard implementation", 18m, 95m, 0m);
        northwindInvoice.MarkAsSent();
        northwindInvoice.RecordPayment(
            northwindInvoice.TotalAmount,
            PaymentMethod.CreditCard,
            "DEMO-PAY-NORTHWIND-001",
            today);

        var acceptedProposal = new Proposal(
            workspaceId,
            acmeClient.Id,
            "PROP-DEMO-001",
            "Automation Retainer Proposal",
            "Monthly automation support, reporting maintenance, and integration hardening.",
            today.AddDays(30),
            "USD");
        acceptedProposal.AddItem("Monthly automation retainer", 1m, 2400m, 0m);
        acceptedProposal.MarkAsSent();
        acceptedProposal.Accept(today);
        acceptedProposal.MarkConverted(retainerProject.Id);

        var expiredProposal = new Proposal(
            workspaceId,
            northwindClient.Id,
            "PROP-DEMO-002",
            "Reporting Expansion Proposal",
            "Additional executive reporting surfaces for the internal dashboard.",
            today.AddDays(-5),
            "USD");
        expiredProposal.AddItem("Reporting expansion discovery", 1m, 1800m, 0m);
        expiredProposal.MarkAsSent();
        expiredProposal.MarkExpired(today);

        var notifications = new[]
        {
            new Notification(
                workspaceId,
                userId,
                NotificationType.InvoiceOverdue,
                "Invoice overdue",
                "DEMO-2026-001 has an overdue outstanding balance.",
                "Invoice",
                acmeInvoice.Id,
                $"demo:invoice-overdue:{acmeInvoice.Id}"),
            new Notification(
                workspaceId,
                userId,
                NotificationType.ProposalConvertedToProject,
                "Proposal converted",
                "Automation Retainer Proposal was accepted and converted to a project.",
                "Proposal",
                acceptedProposal.Id,
                $"demo:proposal-converted:{acceptedProposal.Id}"),
            new Notification(
                workspaceId,
                userId,
                NotificationType.TaskAssigned,
                "Task assigned",
                "API authentication hardening is assigned to the demo user.",
                "ProjectTask",
                tasks[0].Id,
                $"demo:task-assigned:{tasks[0].Id}")
        };

        dbContext.Clients.AddRange(acmeClient, northwindClient);
        dbContext.Projects.AddRange(apiProject, retainerProject, dashboardProject);
        dbContext.ProjectTasks.AddRange(tasks);
        dbContext.TimeEntries.AddRange(timeEntries);
        dbContext.Invoices.AddRange(acmeInvoice, northwindInvoice);
        dbContext.Proposals.AddRange(acceptedProposal, expiredProposal);
        dbContext.Notifications.AddRange(notifications);
    }

    private static ProjectTask[] CreateDemoTasks(
        Guid workspaceId,
        Guid userId,
        Guid apiProjectId,
        Guid retainerProjectId,
        Guid dashboardProjectId,
        DateOnly today)
    {
        return
        [
            CreateTask(
                workspaceId,
                apiProjectId,
                "Harden authentication flow",
                "Review token rotation and authorization paths.",
                ProjectTaskPriority.High,
                ProjectTaskStatus.InProgress,
                today.AddDays(3),
                userId),
            CreateTask(
                workspaceId,
                apiProjectId,
                "Implement billing lifecycle",
                "Invoice lifecycle and payment recording for demo flow.",
                ProjectTaskPriority.High,
                ProjectTaskStatus.Done,
                today.AddDays(-4),
                userId),
            CreateTask(
                workspaceId,
                apiProjectId,
                "Add reporting endpoints",
                "Dashboard, revenue, client summary, and project performance reports.",
                ProjectTaskPriority.Medium,
                ProjectTaskStatus.Done,
                today.AddDays(-2),
                userId),
            CreateTask(
                workspaceId,
                apiProjectId,
                "Wire notification queries",
                "Per-user notification list and unread count.",
                ProjectTaskPriority.Medium,
                ProjectTaskStatus.InReview,
                today.AddDays(5),
                userId),
            CreateTask(
                workspaceId,
                retainerProjectId,
                "Plan automation backlog",
                "Prioritize recurring maintenance and reporting tasks.",
                ProjectTaskPriority.Medium,
                ProjectTaskStatus.Todo,
                today.AddDays(9),
                userId),
            CreateTask(
                workspaceId,
                retainerProjectId,
                "Prepare monthly status report",
                "Summarize completed retainer work for the client.",
                ProjectTaskPriority.Low,
                ProjectTaskStatus.Todo,
                today.AddDays(14),
                userId),
            CreateTask(
                workspaceId,
                dashboardProjectId,
                "Design dashboard metrics",
                "Define operational metrics and report cards.",
                ProjectTaskPriority.High,
                ProjectTaskStatus.Done,
                today.AddDays(-12),
                userId),
            CreateTask(
                workspaceId,
                dashboardProjectId,
                "Build revenue widgets",
                "Implement paid revenue and outstanding invoice panels.",
                ProjectTaskPriority.High,
                ProjectTaskStatus.Done,
                today.AddDays(-7),
                userId),
            CreateTask(
                workspaceId,
                dashboardProjectId,
                "Validate report filters",
                "Verify date range behavior across dashboard reports.",
                ProjectTaskPriority.Medium,
                ProjectTaskStatus.Done,
                today.AddDays(-3),
                userId),
            CreateTask(
                workspaceId,
                dashboardProjectId,
                "Document dashboard handoff",
                "Write client-facing handoff notes for dashboard usage.",
                ProjectTaskPriority.Low,
                ProjectTaskStatus.Done,
                today.AddDays(-1),
                userId)
        ];
    }

    private static TimeEntry[] CreateDemoTimeEntries(
        Guid workspaceId,
        Guid userId,
        Guid apiProjectId,
        Guid retainerProjectId,
        Guid dashboardProjectId,
        IReadOnlyList<ProjectTask> tasks,
        DateTime reportDate)
    {
        return
        [
            TimeEntry.CreateManual(
                workspaceId,
                apiProjectId,
                tasks[0].Id,
                userId,
                reportDate.AddHours(8),
                480,
                "Authentication and workspace authorization hardening."),
            TimeEntry.CreateManual(
                workspaceId,
                apiProjectId,
                tasks[1].Id,
                userId,
                reportDate.AddDays(1).AddHours(8),
                420,
                "Billing lifecycle implementation."),
            TimeEntry.CreateManual(
                workspaceId,
                apiProjectId,
                tasks[2].Id,
                userId,
                reportDate.AddDays(2).AddHours(8),
                600,
                "Reporting endpoint implementation."),
            TimeEntry.CreateManual(
                workspaceId,
                apiProjectId,
                tasks[3].Id,
                userId,
                reportDate.AddDays(3).AddHours(8),
                600,
                "Notification query and worker review."),
            TimeEntry.CreateManual(
                workspaceId,
                apiProjectId,
                tasks[0].Id,
                userId,
                reportDate.AddDays(4).AddHours(8),
                420,
                "Demo flow cleanup for API project."),
            TimeEntry.CreateManual(
                workspaceId,
                retainerProjectId,
                tasks[4].Id,
                userId,
                reportDate.AddDays(5).AddHours(8),
                240,
                "Retainer backlog planning."),
            TimeEntry.CreateManual(
                workspaceId,
                dashboardProjectId,
                tasks[6].Id,
                userId,
                reportDate.AddHours(9),
                360,
                "Dashboard metric design."),
            TimeEntry.CreateManual(
                workspaceId,
                dashboardProjectId,
                tasks[7].Id,
                userId,
                reportDate.AddDays(1).AddHours(9),
                480,
                "Revenue widget build."),
            TimeEntry.CreateManual(
                workspaceId,
                dashboardProjectId,
                tasks[8].Id,
                userId,
                reportDate.AddDays(2).AddHours(9),
                240,
                "Report filter validation.")
        ];
    }

    private static ProjectTask CreateTask(
        Guid workspaceId,
        Guid projectId,
        string title,
        string description,
        ProjectTaskPriority priority,
        ProjectTaskStatus status,
        DateOnly dueDate,
        Guid assignedToUserId)
    {
        var task = new ProjectTask(
            workspaceId,
            projectId,
            title,
            description,
            priority,
            dueDate,
            assignedToUserId);
        task.ChangeStatus(status);

        return task;
    }

    private async Task RemoveExistingDemoDataAsync(CancellationToken cancellationToken)
    {
        var workspaceIds = await dbContext.Workspaces
            .Where(workspace => workspace.Slug == DemoWorkspaceSlug)
            .Select(workspace => workspace.Id)
            .ToListAsync(cancellationToken);

        if (workspaceIds.Count > 0)
        {
            var invoiceIds = await dbContext.Invoices
                .Where(invoice => workspaceIds.Contains(invoice.WorkspaceId))
                .Select(invoice => invoice.Id)
                .ToListAsync(cancellationToken);
            var proposalIds = await dbContext.Proposals
                .Where(proposal => workspaceIds.Contains(proposal.WorkspaceId))
                .Select(proposal => proposal.Id)
                .ToListAsync(cancellationToken);

            dbContext.PaymentRecords.RemoveRange(await dbContext.PaymentRecords
                .Where(payment => invoiceIds.Contains(payment.InvoiceId))
                .ToListAsync(cancellationToken));
            dbContext.InvoiceItems.RemoveRange(await dbContext.InvoiceItems
                .Where(item => invoiceIds.Contains(item.InvoiceId))
                .ToListAsync(cancellationToken));
            dbContext.Invoices.RemoveRange(await dbContext.Invoices
                .Where(invoice => workspaceIds.Contains(invoice.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.ProposalItems.RemoveRange(await dbContext.ProposalItems
                .Where(item => proposalIds.Contains(item.ProposalId))
                .ToListAsync(cancellationToken));
            dbContext.Proposals.RemoveRange(await dbContext.Proposals
                .Where(proposal => workspaceIds.Contains(proposal.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.Notifications.RemoveRange(await dbContext.Notifications
                .Where(notification => workspaceIds.Contains(notification.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.TimeEntries.RemoveRange(await dbContext.TimeEntries
                .Where(timeEntry => workspaceIds.Contains(timeEntry.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.ProjectTasks.RemoveRange(await dbContext.ProjectTasks
                .Where(task => workspaceIds.Contains(task.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.Projects.RemoveRange(await dbContext.Projects
                .Where(project => workspaceIds.Contains(project.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.Clients.RemoveRange(await dbContext.Clients
                .Where(client => workspaceIds.Contains(client.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.WorkspaceMembers.RemoveRange(await dbContext.WorkspaceMembers
                .Where(member => workspaceIds.Contains(member.WorkspaceId))
                .ToListAsync(cancellationToken));
            dbContext.Workspaces.RemoveRange(await dbContext.Workspaces
                .Where(workspace => workspaceIds.Contains(workspace.Id))
                .ToListAsync(cancellationToken));

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var demoUserIds = await dbContext.Users
            .Where(user => user.Email == User.NormalizeEmail(DemoEmail))
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        if (demoUserIds.Count == 0)
        {
            return;
        }

        dbContext.RefreshTokens.RemoveRange(await dbContext.RefreshTokens
            .Where(refreshToken => demoUserIds.Contains(refreshToken.UserId))
            .ToListAsync(cancellationToken));
        dbContext.WorkspaceMembers.RemoveRange(await dbContext.WorkspaceMembers
            .Where(member => demoUserIds.Contains(member.UserId))
            .ToListAsync(cancellationToken));
        dbContext.Users.RemoveRange(await dbContext.Users
            .Where(user => demoUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime CreateReportDate()
    {
        var now = DateTime.UtcNow;
        var day = Math.Min(now.Day, 5);

        return new DateTime(now.Year, now.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
