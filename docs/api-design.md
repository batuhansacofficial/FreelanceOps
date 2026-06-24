# API Design

The first stable endpoints are:

```http
GET /health
GET /openapi/v1.json
GET /swagger
```

Auth endpoints:

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

Workspace endpoints:

```http
POST   /api/workspaces
GET    /api/workspaces
GET    /api/workspaces/{workspaceId}
PUT    /api/workspaces/{workspaceId}
DELETE /api/workspaces/{workspaceId}
GET    /api/workspaces/{workspaceId}/members
POST   /api/workspaces/{workspaceId}/members
PATCH  /api/workspaces/{workspaceId}/members/{memberId}/role
DELETE /api/workspaces/{workspaceId}/members/{memberId}
```

Every workspace endpoint requires a valid bearer token. Endpoints with a `workspaceId` route value also check active membership. Member-management endpoints require `Owner` or `Admin`, except workspace deletion, which requires `Owner`.

Client endpoints:

```http
POST   /api/workspaces/{workspaceId}/clients
GET    /api/workspaces/{workspaceId}/clients?page=1&pageSize=20&search=acme
GET    /api/workspaces/{workspaceId}/clients/{clientId}
PUT    /api/workspaces/{workspaceId}/clients/{clientId}
DELETE /api/workspaces/{workspaceId}/clients/{clientId}
```

Client list responses include:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

Client read endpoints require `Owner`, `Admin`, or `Member`. Client create, update, and delete endpoints require `Owner` or `Admin`.

Every client query filters by `WorkspaceId` and `IsDeleted = false`. Client detail, update, and delete lookups also filter by `ClientId`, so a known client id cannot be read through another workspace route.

Project endpoints:

```http
POST   /api/workspaces/{workspaceId}/projects
GET    /api/workspaces/{workspaceId}/projects?page=1&pageSize=20&search=api&status=Active&clientId={clientId}
GET    /api/workspaces/{workspaceId}/projects/{projectId}
PUT    /api/workspaces/{workspaceId}/projects/{projectId}
PATCH  /api/workspaces/{workspaceId}/projects/{projectId}/status
DELETE /api/workspaces/{workspaceId}/projects/{projectId}
```

Task endpoints:

```http
POST   /api/workspaces/{workspaceId}/projects/{projectId}/tasks
GET    /api/workspaces/{workspaceId}/projects/{projectId}/tasks?page=1&pageSize=20&status=Todo&assignedToUserId={userId}
GET    /api/workspaces/{workspaceId}/tasks/{taskId}
PUT    /api/workspaces/{workspaceId}/tasks/{taskId}
PATCH  /api/workspaces/{workspaceId}/tasks/{taskId}/status
DELETE /api/workspaces/{workspaceId}/tasks/{taskId}
```

Project read endpoints require `Owner`, `Admin`, or `Member`. Project create, update, status change, and delete endpoints require `Owner` or `Admin`.

Task read, create, update, and status change endpoints require active workspace membership. Task delete requires `Owner` or `Admin`.

Project creation validates that the selected client belongs to the same workspace and is not deleted. Task creation and update validate that the project belongs to the route workspace and that `assignedToUserId`, when provided, is an active member of the same workspace.

Time tracking endpoints:

```http
POST   /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/start
POST   /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/manual
POST   /api/workspaces/{workspaceId}/time-entries/{timeEntryId}/stop
GET    /api/workspaces/{workspaceId}/time-entries
GET    /api/workspaces/{workspaceId}/projects/{projectId}/time-entries
GET    /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries
PUT    /api/workspaces/{workspaceId}/time-entries/{timeEntryId}
DELETE /api/workspaces/{workspaceId}/time-entries/{timeEntryId}
GET    /api/workspaces/{workspaceId}/reports/time-summary
```

All workspace members can start timers and create manual entries for active tasks. A user can have only one active timer across all workspaces.

Owner/Admin users can list all workspace entries and manage any entry. Members can list only their own entries, stop only their own timer, update only their own manual entries, and delete only their own entries.

Time-entry lists support `page`, `pageSize`, `userId`, `projectId`, `taskId`, `from`, and `to`. The summary endpoint supports date, project, and task filters and returns totals grouped by project and user.

Billing endpoints:

```http
POST   /api/workspaces/{workspaceId}/invoices
GET    /api/workspaces/{workspaceId}/invoices?page=1&pageSize=20&status=Draft&clientId={clientId}&projectId={projectId}&search=INV-2026
GET    /api/workspaces/{workspaceId}/invoices/{invoiceId}
PUT    /api/workspaces/{workspaceId}/invoices/{invoiceId}
DELETE /api/workspaces/{workspaceId}/invoices/{invoiceId}
PATCH  /api/workspaces/{workspaceId}/invoices/{invoiceId}/send
PATCH  /api/workspaces/{workspaceId}/invoices/{invoiceId}/cancel
POST   /api/workspaces/{workspaceId}/invoices/{invoiceId}/payments
GET    /api/workspaces/{workspaceId}/invoices/{invoiceId}/payments
```

Billing endpoints require `Owner` or `Admin`. Members receive `403`.

Invoice creation validates the client, optional project, and project-client relationship within the route workspace. Draft invoices can be edited or soft-deleted. Draft invoices can be sent, payments reduce balance, and a full payment changes status to `Paid`. Paid invoices cannot be cancelled.

Proposal endpoints:

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

Proposal endpoints require `Owner` or `Admin`. Members receive `403`.

Proposal creation validates the client inside the route workspace. Draft proposals can be edited or soft-deleted. Draft proposals can be sent, sent proposals can be accepted or rejected, expired proposals cannot be accepted, and accepted proposals can be converted to one draft project.

Report endpoints:

```http
GET /api/workspaces/{workspaceId}/reports/dashboard
GET /api/workspaces/{workspaceId}/reports/revenue?from=2026-06-01&to=2026-06-30&groupBy=month
GET /api/workspaces/{workspaceId}/reports/client-summary?from=2026-06-01&to=2026-06-30
GET /api/workspaces/{workspaceId}/reports/project-performance?from=2026-06-01&to=2026-06-30
```

Business reports require `Owner` or `Admin`; Members receive `403`. Date ranges default to the current UTC month, are inclusive, and cannot exceed 366 days. Revenue is calculated from payment records and grouped by invoice currency. Tracked-time metrics exclude active timers.

Notification endpoints:

```http
GET   /api/workspaces/{workspaceId}/notifications?page=1&pageSize=20&isRead=false
GET   /api/workspaces/{workspaceId}/notifications/unread-count
PATCH /api/workspaces/{workspaceId}/notifications/{notificationId}/read
PATCH /api/workspaces/{workspaceId}/notifications/read-all
```

Notification endpoints require active workspace membership. A requester can only list, count, and mark their own notifications. Owner/Admin users do not get access to another user's notifications.
