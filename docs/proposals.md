# Proposals

Proposals are workspace-scoped commercial offers that can be accepted and converted into projects. Every proposal endpoint requires the requester to be an active workspace `Owner` or `Admin`.

## Lifecycle

```text
Draft -> Sent -> Accepted -> Project
Draft -> Sent -> Rejected
Draft -> Cancelled
Sent  -> Cancelled
```

Rules:

- Only draft proposals can be edited or soft-deleted.
- Draft proposals require at least one item before they can be sent.
- Sent proposals can be accepted or rejected.
- Expired proposals cannot be accepted.
- Accepted, rejected, and cancelled proposals cannot be converted unless the status is `Accepted`.
- An accepted proposal can be converted to a project once.

## Endpoints

```http
POST   /api/workspaces/{workspaceId}/proposals
GET    /api/workspaces/{workspaceId}/proposals?page=1&pageSize=20&status=Sent&clientId={clientId}&search=backend
GET    /api/workspaces/{workspaceId}/proposals/{proposalId}
PUT    /api/workspaces/{workspaceId}/proposals/{proposalId}
DELETE /api/workspaces/{workspaceId}/proposals/{proposalId}
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/send
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/accept
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/reject
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/cancel
POST   /api/workspaces/{workspaceId}/proposals/{proposalId}/convert-to-project
```

List filters:

```text
status
clientId
search: proposalNumber or title
page
pageSize
```

## Totals

Each item calculates:

```text
subtotal = quantity * unitPrice
tax = subtotal * taxRate / 100
total = subtotal + tax
```

Item monetary values are rounded to two decimal places. Proposal totals are the sums of item totals. PostgreSQL columns use explicit decimal precision.

## Proposal Numbers

Proposal numbers are generated per workspace and creation year:

```text
PROP-2026-0001
PROP-2026-0002
```

The current implementation counts proposals in the workspace/year and adds one. It includes soft-deleted proposals to avoid number reuse.

This strategy is acceptable for the current MVP but is not safe for high-concurrency production workloads. A future version should use a locked workspace sequence or database sequence.

## Authorization

Proposal endpoints require `Owner` or `Admin`. Members receive `403`.

Proposal creation validates:

```text
Requester is Owner/Admin in the workspace.
Client belongs to the workspace and is not deleted.
At least one item is provided.
```

Proposal detail, update, deletion, lifecycle, and conversion operations use both `WorkspaceId` and `ProposalId`.

## Convert To Project

Conversion requires:

```text
Proposal belongs to the route workspace.
Proposal status is Accepted.
Proposal has not already been converted.
Client still belongs to the workspace and is not deleted.
```

The created project uses:

```text
WorkspaceId = Proposal.WorkspaceId
ClientId = Proposal.ClientId
Name = Proposal.Title
Description = Proposal.Scope
Status = Draft
```

The proposal stores `ConvertedProjectId` after a successful conversion.

## Tenant Isolation

Every proposal query filters by route `WorkspaceId` and `IsDeleted = false`. Proposal creation and conversion also validate that the client is active inside the same workspace. A known proposal or client id cannot be used through another workspace route.

## Known Limitations

- Proposal numbers use a simple workspace/year count strategy and are not concurrency-safe.
- PDF export is not implemented yet.
- Proposal approval is represented through API status changes; no public client portal exists yet.
- Email delivery is not implemented.
