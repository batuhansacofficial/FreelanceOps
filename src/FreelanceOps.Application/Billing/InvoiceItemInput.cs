namespace FreelanceOps.Application.Billing;

public sealed record InvoiceItemInput(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);
