# FreelanceOps

Production-style multi-tenant SaaS backend for freelancers and small agencies.

## Current Status

Initial solution setup is in place:

- ASP.NET Core Web API
- Application, Domain, Infrastructure, and Worker projects
- PostgreSQL EF Core configuration
- Docker Compose for PostgreSQL and Redis
- Health check endpoint at `/health`
- Auth endpoints for register, login, refresh-token rotation, logout, and current user
- Workspace and workspace-member endpoints with role-based access checks
- Workspace-scoped client CRUD with pagination, search, and soft delete
- Workspace-scoped project and task management with status changes, filters, pagination, assignment checks, and soft delete
- Workspace-scoped timer/manual time entries with role-based visibility, summaries, and soft delete
- Owner/Admin-only invoice management with item totals, lifecycle transitions, partial payments, and workspace numbering
- Owner/Admin-only proposal management with item totals, lifecycle transitions, workspace numbering, and project conversion
- Owner/Admin-only dashboard, revenue, client summary, and project performance reports
- Database-backed per-user workspace notifications and due-date monitoring jobs
- Integration tests using xUnit, WebApplicationFactory, PostgreSQL Testcontainers, and FluentAssertions
- OpenAPI document and Swagger UI in development
- Global exception middleware

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- Docker Compose

## Architecture

The project uses a modular monolith with vertical slices planned inside the application layer. The initial dependency direction is:

```text
Api -> Application
Api -> Infrastructure
Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
Worker -> Application
Worker -> Infrastructure
```

`Domain` has no project dependencies.

## Local Setup

Start infrastructure:

```bash
docker compose up -d
```

Restore local .NET tools:

```bash
dotnet tool restore
```

Run the API:

```bash
dotnet run --project src/FreelanceOps.Api/FreelanceOps.Api.csproj
```

Swagger UI is available in development at:

```text
/swagger
```

Health check:

```text
/health
```

Run tests:

```bash
dotnet test FreelanceOps.sln --configuration Release
```

Auth endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

Workspace endpoints:

```text
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

Client endpoints:

```text
POST   /api/workspaces/{workspaceId}/clients
GET    /api/workspaces/{workspaceId}/clients?page=1&pageSize=20&search=acme
GET    /api/workspaces/{workspaceId}/clients/{clientId}
PUT    /api/workspaces/{workspaceId}/clients/{clientId}
DELETE /api/workspaces/{workspaceId}/clients/{clientId}
```

Project endpoints:

```text
POST   /api/workspaces/{workspaceId}/projects
GET    /api/workspaces/{workspaceId}/projects?page=1&pageSize=20&search=api&status=Active&clientId={clientId}
GET    /api/workspaces/{workspaceId}/projects/{projectId}
PUT    /api/workspaces/{workspaceId}/projects/{projectId}
PATCH  /api/workspaces/{workspaceId}/projects/{projectId}/status
DELETE /api/workspaces/{workspaceId}/projects/{projectId}
```

Task endpoints:

```text
POST   /api/workspaces/{workspaceId}/projects/{projectId}/tasks
GET    /api/workspaces/{workspaceId}/projects/{projectId}/tasks?page=1&pageSize=20&status=Todo&assignedToUserId={userId}
GET    /api/workspaces/{workspaceId}/tasks/{taskId}
PUT    /api/workspaces/{workspaceId}/tasks/{taskId}
PATCH  /api/workspaces/{workspaceId}/tasks/{taskId}/status
DELETE /api/workspaces/{workspaceId}/tasks/{taskId}
```

Time tracking endpoints:

```text
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

Billing endpoints:

```text
POST   /api/workspaces/{workspaceId}/invoices
GET    /api/workspaces/{workspaceId}/invoices
GET    /api/workspaces/{workspaceId}/invoices/{invoiceId}
PUT    /api/workspaces/{workspaceId}/invoices/{invoiceId}
DELETE /api/workspaces/{workspaceId}/invoices/{invoiceId}
PATCH  /api/workspaces/{workspaceId}/invoices/{invoiceId}/send
PATCH  /api/workspaces/{workspaceId}/invoices/{invoiceId}/cancel
POST   /api/workspaces/{workspaceId}/invoices/{invoiceId}/payments
GET    /api/workspaces/{workspaceId}/invoices/{invoiceId}/payments
```

Proposal endpoints:

```text
POST   /api/workspaces/{workspaceId}/proposals
GET    /api/workspaces/{workspaceId}/proposals
GET    /api/workspaces/{workspaceId}/proposals/{proposalId}
PUT    /api/workspaces/{workspaceId}/proposals/{proposalId}
DELETE /api/workspaces/{workspaceId}/proposals/{proposalId}
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/send
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/accept
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/reject
PATCH  /api/workspaces/{workspaceId}/proposals/{proposalId}/cancel
POST   /api/workspaces/{workspaceId}/proposals/{proposalId}/convert-to-project
```

Report endpoints:

```text
GET /api/workspaces/{workspaceId}/reports/dashboard
GET /api/workspaces/{workspaceId}/reports/revenue
GET /api/workspaces/{workspaceId}/reports/client-summary
GET /api/workspaces/{workspaceId}/reports/project-performance
```

Notification endpoints:

```text
GET   /api/workspaces/{workspaceId}/notifications
GET   /api/workspaces/{workspaceId}/notifications/unread-count
PATCH /api/workspaces/{workspaceId}/notifications/{notificationId}/read
PATCH /api/workspaces/{workspaceId}/notifications/read-all
```

## Known Limitations

- Workspace isolation exists for workspace, member, client, project, task, time-tracking, billing, proposal, and reporting endpoints.
- Invoice PDF export and external payment-provider integration are not implemented.
- Invoice numbering uses a workspace/year count and is not safe for high-concurrency production workloads.
- Proposal PDF export, public client approval portal, and email delivery are not implemented.
- Proposal numbering uses a workspace/year count and is not safe for high-concurrency production workloads.
- Project performance is not profitability because labor costs and expenses are not tracked.
- Multi-currency revenue is grouped without exchange-rate conversion.
- Active timers are excluded from reports until stopped.
- Real-time SignalR notification delivery, email sending, payment processing, and file storage are not implemented.
- Frontend is not part of the initial MVP.
