using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Workspaces;

public sealed class WorkspaceMember
{
    private WorkspaceMember()
    {
    }

    private WorkspaceMember(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static WorkspaceMember CreateOwner(Guid workspaceId, Guid userId)
    {
        return new WorkspaceMember(workspaceId, userId, WorkspaceRole.Owner);
    }

    public static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        if (role == WorkspaceRole.Owner)
        {
            throw new DomainException("Owner role cannot be assigned directly.");
        }

        return new WorkspaceMember(workspaceId, userId, role);
    }

    public void ChangeRole(WorkspaceRole role)
    {
        if (Role == WorkspaceRole.Owner)
        {
            throw new DomainException("Owner role cannot be changed.");
        }

        if (role == WorkspaceRole.Owner)
        {
            throw new DomainException("Owner role cannot be assigned directly.");
        }

        Role = role;
    }

    public void Deactivate()
    {
        if (Role == WorkspaceRole.Owner)
        {
            throw new DomainException("Owner cannot be removed from workspace.");
        }

        IsActive = false;
    }

    public void Reactivate(WorkspaceRole role)
    {
        if (role == WorkspaceRole.Owner)
        {
            throw new DomainException("Owner role cannot be assigned directly.");
        }

        Role = role;
        IsActive = true;
        JoinedAtUtc = DateTime.UtcNow;
    }
}
