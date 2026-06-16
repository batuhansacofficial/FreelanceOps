namespace FreelanceOps.Domain.Projects;

public sealed class ProjectTask
{
    private ProjectTask()
    {
    }

    public ProjectTask(
        Guid workspaceId,
        Guid projectId,
        string title,
        string? description,
        ProjectTaskPriority priority,
        DateOnly? dueDate,
        Guid? assignedToUserId)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        ProjectId = projectId;
        Title = title.Trim();
        Description = NormalizeOptional(description);
        Status = ProjectTaskStatus.Todo;
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProjectTaskStatus Status { get; private set; }
    public ProjectTaskPriority Priority { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Update(
        string title,
        string? description,
        ProjectTaskPriority priority,
        DateOnly? dueDate,
        Guid? assignedToUserId)
    {
        Title = title.Trim();
        Description = NormalizeOptional(description);
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeStatus(ProjectTaskStatus status)
    {
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
