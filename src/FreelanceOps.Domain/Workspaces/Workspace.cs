namespace FreelanceOps.Domain.Workspaces;

public sealed class Workspace
{
    private readonly List<WorkspaceMember> _members = [];

    private Workspace()
    {
    }

    public Workspace(string name, string slug, Guid ownerUserId)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        OwnerUserId = ownerUserId;
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;

        _members.Add(WorkspaceMember.CreateOwner(Id, ownerUserId));
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public Guid OwnerUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<WorkspaceMember> Members => _members;

    public void Rename(string name)
    {
        Name = name.Trim();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
