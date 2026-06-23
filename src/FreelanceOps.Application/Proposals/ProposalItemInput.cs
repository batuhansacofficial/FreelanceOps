namespace FreelanceOps.Application.Proposals;

public sealed record ProposalItemInput(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);
