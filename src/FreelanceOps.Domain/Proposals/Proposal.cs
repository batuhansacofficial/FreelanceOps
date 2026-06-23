using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Proposals;

public sealed class Proposal
{
    private readonly List<ProposalItem> _items = [];

    private Proposal()
    {
    }

    public Proposal(
        Guid workspaceId,
        Guid clientId,
        string proposalNumber,
        string title,
        string scope,
        DateOnly validUntil,
        string currency)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new DomainException("Scope is required.");
        }

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        ClientId = clientId;
        ProposalNumber = proposalNumber.Trim();
        Title = title.Trim();
        Scope = scope.Trim();
        Status = ProposalStatus.Draft;
        ValidUntil = validUntil;
        Currency = currency.Trim().ToUpperInvariant();
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? ConvertedProjectId { get; private set; }
    public string ProposalNumber { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Scope { get; private set; } = null!;
    public ProposalStatus Status { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public IReadOnlyCollection<ProposalItem> Items => _items;

    public void AddItem(
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate)
    {
        EnsureEditable();

        _items.Add(new ProposalItem(Id, description, quantity, unitPrice, taxRate));

        RecalculateTotals();
    }

    public void ReplaceItems(IEnumerable<ProposalItem> items)
    {
        EnsureEditable();

        _items.Clear();
        _items.AddRange(items);

        RecalculateTotals();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string title,
        string scope,
        DateOnly validUntil,
        string currency)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new DomainException("Scope is required.");
        }

        Title = title.Trim();
        Scope = scope.Trim();
        ValidUntil = validUntil;
        Currency = currency.Trim().ToUpperInvariant();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        if (Status != ProposalStatus.Draft)
        {
            throw new DomainException("Only draft proposals can be sent.");
        }

        if (_items.Count == 0)
        {
            throw new DomainException("Proposal must contain at least one item.");
        }

        Status = ProposalStatus.Sent;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Accept(DateOnly today)
    {
        if (Status != ProposalStatus.Sent)
        {
            throw new DomainException("Only sent proposals can be accepted.");
        }

        if (ValidUntil < today)
        {
            throw new DomainException("Expired proposal cannot be accepted.");
        }

        Status = ProposalStatus.Accepted;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ProposalStatus.Sent)
        {
            throw new DomainException("Only sent proposals can be rejected.");
        }

        Status = ProposalStatus.Rejected;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ProposalStatus.Cancelled)
        {
            return;
        }

        if (Status is not (ProposalStatus.Draft or ProposalStatus.Sent))
        {
            throw new DomainException("Only draft or sent proposals can be cancelled.");
        }

        Status = ProposalStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkConverted(Guid projectId)
    {
        if (Status != ProposalStatus.Accepted)
        {
            throw new DomainException("Only accepted proposals can be converted to projects.");
        }

        if (ConvertedProjectId.HasValue)
        {
            throw new DomainException("Proposal has already been converted to a project.");
        }

        ConvertedProjectId = projectId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (Status != ProposalStatus.Draft)
        {
            throw new DomainException("Only draft proposals can be deleted.");
        }

        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status != ProposalStatus.Draft)
        {
            throw new DomainException("Only draft proposals can be edited.");
        }
    }

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Sum(item => item.SubtotalAmount);
        TaxAmount = _items.Sum(item => item.TaxAmount);
        TotalAmount = _items.Sum(item => item.TotalAmount);
    }
}
