namespace FreelanceOps.Domain.Billing;

public sealed class PaymentRecord
{
    private PaymentRecord()
    {
    }

    public PaymentRecord(
        Guid invoiceId,
        decimal amount,
        PaymentMethod method,
        string? reference,
        DateOnly paidAt)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Amount = amount;
        Method = method;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        PaidAt = paidAt;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public DateOnly PaidAt { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
