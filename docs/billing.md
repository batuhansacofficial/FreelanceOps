# Billing

Billing is workspace-scoped financial data. Every invoice and payment endpoint requires the requester to be an active workspace `Owner` or `Admin`.

## Invoice Lifecycle

```text
Draft -> Sent -> Paid
Draft -> Cancelled
Sent  -> Cancelled
```

Rules:

- Only draft invoices can be edited or soft-deleted.
- Draft invoices require at least one item before they can be sent.
- Payments cannot exceed the remaining balance.
- A full payment changes the invoice status to `Paid`.
- Paid invoices cannot be cancelled.
- Cancelled invoices cannot receive payments.

## Totals

Each item calculates:

```text
subtotal = quantity * unitPrice
tax = subtotal * taxRate / 100
total = subtotal + tax
```

Item monetary values are rounded to two decimal places. Invoice totals are the sums of item totals. PostgreSQL columns use explicit decimal precision.

## Invoice Numbers

Invoice numbers are generated per workspace and issue year:

```text
INV-2026-0001
INV-2026-0002
```

The current implementation counts invoices in the workspace/year and adds one. It includes soft-deleted invoices to avoid number reuse.

This strategy is acceptable for the current MVP but is not safe for high-concurrency production workloads. A future version should use a locked workspace sequence or database sequence.

## Tenant Integrity

Invoice creation validates:

```text
Client belongs to workspace and is not deleted.
Optional project belongs to workspace and is not deleted.
Project.ClientId equals Invoice.ClientId.
```

Invoice detail, updates, lifecycle changes, deletion, and payment operations use both `WorkspaceId` and `InvoiceId`.

## Payments

Payments support:

```text
Cash
BankTransfer
CreditCard
PayPal
Other
```

Partial payments are retained as payment records and reduce `BalanceDue`. When `BalanceDue` reaches zero, the invoice becomes `Paid`.

## Known Limitations

- PDF export is not implemented.
- External payment providers are not integrated.
- Overdue is computed from `Sent`, due date, and remaining balance; there is no background overdue job.
- Invoice number generation is not concurrency-safe enough for high-volume production use.
