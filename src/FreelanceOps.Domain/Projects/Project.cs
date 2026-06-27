using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Projects;

public sealed class Project
{
    private Project()
    {
    }

    public Project(
        Guid workspaceId,
        Guid clientId,
        string name,
        string? description,
        DateOnly? startDate,
        DateOnly? deadline)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        ClientId = clientId;
        Name = name.Trim();
        Description = NormalizeOptional(description);
        Status = ProjectStatus.Draft;
        StartDate = startDate;
        Deadline = deadline;
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? Deadline { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Update(
        string name,
        string? description,
        DateOnly? startDate,
        DateOnly? deadline)
    {
        Name = name.Trim();
        Description = NormalizeOptional(description);
        StartDate = startDate;
        Deadline = deadline;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeStatus(ProjectStatus status)
    {
        if (Status == ProjectStatus.Cancelled)
        {
            throw new DomainException("Cancelled project status cannot be changed.");
        }

        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
