# Dashboard And Reports

Business reports are available under:

```text
/api/workspaces/{workspaceId}/reports
```

All report endpoints require an authenticated workspace `Owner` or `Admin`. Members and non-members receive `403`.

## Endpoints

```http
GET /api/workspaces/{workspaceId}/reports/dashboard
GET /api/workspaces/{workspaceId}/reports/revenue?from=2026-06-01&to=2026-06-30&groupBy=month
GET /api/workspaces/{workspaceId}/reports/client-summary?from=2026-06-01&to=2026-06-30
GET /api/workspaces/{workspaceId}/reports/project-performance?from=2026-06-01&to=2026-06-30
```

`from` defaults to the first day of the current UTC month. `to` defaults to the current UTC date. Date ranges are inclusive, cannot be reversed, and cannot exceed 366 days.

The revenue endpoint supports `groupBy=day` and `groupBy=month`.

## Dashboard Metrics

- `totalClients`: current non-deleted workspace clients.
- `activeProjects`: current non-deleted projects with `Active` status.
- `completedProjects`: current non-deleted projects with `Completed` status.
- `trackedMinutesThisMonth`: stopped or manual entries started during the current UTC month.
- `trackedHoursThisMonth`: tracked minutes divided by 60 and rounded to two decimals.
- `paidRevenueThisMonth`: recorded payment amounts during the current UTC month.
- `paidRevenueThisMonthByCurrency`: the same payment revenue separated by invoice currency.
- `outstandingInvoiceAmount`: remaining balance on non-deleted `Sent` invoices.
- `overdueInvoiceCount`: outstanding `Sent` invoices with a due date before today.
- `openInvoiceCount`: non-deleted `Sent` invoices with a positive balance.
- `recentInvoices`: the five most recent non-cancelled invoices.
- `topClientsByRevenue`: the five largest client/currency payment totals for the current month.

For workspaces with multiple currencies, consumers should use `paidRevenueThisMonthByCurrency`. The scalar dashboard total is a nominal sum and does not perform currency conversion.

## Revenue Calculation

Revenue is based on `PaymentRecord.Amount`, not invoice totals. A payment is included when:

```text
Invoice.WorkspaceId matches the route workspace
Invoice.IsDeleted is false
Invoice.Status is not Cancelled
PaymentRecord.PaidAt is inside the requested range
```

Revenue results are grouped by invoice currency. Currency values are never converted or combined in the revenue report.

## Outstanding And Overdue Invoices

Outstanding amount is:

```text
Invoice.TotalAmount - Invoice.PaidAmount
```

Only non-deleted `Sent` invoices with a positive balance are outstanding. Draft invoices are excluded because they have not been sent to the client.

An invoice is overdue when it is outstanding and its due date is before the current UTC date.

## Client Summary

Client summary returns every current non-deleted client in the workspace.

- `projectCount` counts current non-deleted projects for the client.
- `invoiceCount` counts non-cancelled invoices issued inside the report range.
- `paidAmount` sums payments recorded inside the report range.
- `outstandingAmount` sums outstanding invoices issued inside the report range.
- tracked time includes completed entries started inside the report range.

## Project Performance

Project performance is not profitability. It reports:

- completed tracked time started inside the report range,
- non-cancelled invoice totals issued inside the report range,
- payments recorded inside the report range,
- outstanding balances for sent invoices issued inside the report range,
- recorded payment revenue per tracked hour.

`revenuePerTrackedHour` is:

```text
paidAmount / (trackedMinutes / 60)
```

It returns zero when no completed time is tracked.

## Tenant Isolation

Every query filters by the route `WorkspaceId` before aggregating clients, projects, time entries, invoices, or payment records. Related joins also retain the workspace filter. A user who owns multiple workspaces cannot see one workspace's report data through another workspace route.

## Known Limitations

- Project performance is not true profitability because labor cost and expenses are not tracked.
- Revenue is based on recorded payments, not invoice totals.
- Multi-currency revenue is grouped by currency and is not converted.
- Active timers are excluded from tracked time reports until stopped.
- Client and project scalar financial metrics assume a consistent currency within that client or project; no exchange-rate conversion exists.
- Dashboard scalar monthly revenue is a nominal total; use its currency breakdown for multi-currency workspaces.
