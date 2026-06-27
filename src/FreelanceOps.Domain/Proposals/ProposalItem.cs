using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Proposals;

public sealed class ProposalItem
{
    private ProposalItem()
    {
    }

    public ProposalItem(
        Guid proposalId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        if (taxRate is < 0 or > 100)
        {
            throw new DomainException("Tax rate must be between 0 and 100.");
        }

        Id = Guid.NewGuid();
        ProposalId = proposalId;
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        SubtotalAmount = decimal.Round(quantity * unitPrice, 2);
        TaxAmount = decimal.Round(SubtotalAmount * taxRate / 100m, 2);
        TotalAmount = SubtotalAmount + TaxAmount;
    }

    public Guid Id { get; private set; }
    public Guid ProposalId { get; private set; }
    public string Description { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
}
