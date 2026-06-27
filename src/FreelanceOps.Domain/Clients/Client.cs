namespace FreelanceOps.Domain.Clients;

public sealed class Client
{
    private Client()
    {
    }

    public Client(
        Guid workspaceId,
        string name,
        string? email,
        string? companyName,
        string? notes)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        Name = name.Trim();
        Email = NormalizeOptional(email);
        CompanyName = NormalizeOptional(companyName);
        Notes = NormalizeOptional(notes);
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Update(
        string name,
        string? email,
        string? companyName,
        string? notes)
    {
        Name = name.Trim();
        Email = NormalizeOptional(email);
        CompanyName = NormalizeOptional(companyName);
        Notes = NormalizeOptional(notes);
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
