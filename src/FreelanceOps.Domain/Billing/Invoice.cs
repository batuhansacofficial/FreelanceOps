using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Billing;

public sealed class Invoice
{
    private readonly List<InvoiceItem> _items = [];
    private readonly List<PaymentRecord> _payments = [];

    private Invoice()
    {
    }

    public Invoice(
        Guid workspaceId,
        Guid clientId,
        Guid? projectId,
        string invoiceNumber,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string? notes)
    {
        if (dueDate < issueDate)
        {
            throw new DomainException("Due date cannot be before issue date.");
        }

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        ClientId = clientId;
        ProjectId = projectId;
        InvoiceNumber = invoiceNumber.Trim();
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency.Trim().ToUpperInvariant();
        Notes = NormalizeOptional(notes);
        Status = InvoiceStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string InvoiceNumber { get; private set; } = null!;
    public InvoiceStatus Status { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Notes { get; private set; }
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items;
    public IReadOnlyCollection<PaymentRecord> Payments => _payments;
    public decimal BalanceDue => TotalAmount - PaidAmount;

    public bool IsOverdue(DateOnly today)
    {
        return Status == InvoiceStatus.Sent && DueDate < today && BalanceDue > 0;
    }

    public void AddItem(
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate)
    {
        EnsureEditable();

        _items.Add(new InvoiceItem(Id, description, quantity, unitPrice, taxRate));

        RecalculateTotals();
    }

    public void ReplaceItems(IEnumerable<InvoiceItem> items)
    {
        EnsureEditable();

        _items.Clear();
        _items.AddRange(items);

        RecalculateTotals();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string? notes)
    {
        EnsureEditable();

        if (dueDate < issueDate)
        {
            throw new DomainException("Due date cannot be before issue date.");
        }

        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency.Trim().ToUpperInvariant();
        Notes = NormalizeOptional(notes);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainException("Only draft invoices can be sent.");
        }

        if (_items.Count == 0)
        {
            throw new DomainException("Invoice must contain at least one item.");
        }

        Status = InvoiceStatus.Sent;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public PaymentRecord RecordPayment(
        decimal amount,
        PaymentMethod method,
        string? reference,
        DateOnly paidAt)
    {
        if (Status == InvoiceStatus.Cancelled)
        {
            throw new DomainException("Cancelled invoice cannot be paid.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Payment amount must be greater than zero.");
        }

        if (amount > BalanceDue)
        {
            throw new DomainException("Payment amount cannot exceed balance due.");
        }

        var payment = new PaymentRecord(Id, amount, method, reference, paidAt);
        _payments.Add(payment);
        PaidAmount += amount;

        if (BalanceDue == 0)
        {
            Status = InvoiceStatus.Paid;
        }

        UpdatedAtUtc = DateTime.UtcNow;

        return payment;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainException("Paid invoice cannot be cancelled.");
        }

        if (Status == InvoiceStatus.Cancelled)
        {
            return;
        }

        Status = InvoiceStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainException("Only draft invoices can be deleted.");
        }

        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainException("Only draft invoices can be edited.");
        }
    }

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Sum(item => item.SubtotalAmount);
        TaxAmount = _items.Sum(item => item.TaxAmount);
        TotalAmount = _items.Sum(item => item.TotalAmount);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
