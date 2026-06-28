# Security

The security model is implemented in milestones and currently includes the following protections.

## Authentication

- Custom JWT authentication
- Refresh token rotation
- Refresh token hashing before persistence
- Logout with refresh token revocation
- Consistent `401` behavior for unauthenticated or invalid-token requests

## Workspace Authorization

Implemented workspace rules:

- Workspace creator becomes `Owner`
- Active membership is required for workspace access
- `Owner` and `Admin` can manage workspace members
- `Owner` cannot be removed
- `Owner` role cannot be assigned through member endpoints
- `Owner` role cannot be changed

Reusable services:

- `IWorkspaceAccessService`
- `IWorkspaceAuthorizationService`

## Tenant Isolation

Business data is scoped by `WorkspaceId`.

Implemented workspace-scoped modules:

- Clients
- Projects
- Tasks
- Time entries
- Proposals
- Invoices
- Reports
- Notifications

Tenant-safe lookups use route `workspaceId` together with entity IDs to prevent cross-workspace data exposure.

## Financial Access

Billing, proposals, and business reports are restricted to `Owner` and `Admin`.

## Tests

The repository includes integration tests covering:

- Authentication flows
- Workspace membership rules
- Tenant isolation
- Role-based authorization
- Cross-workspace access rejection
- Billing and proposal lifecycle rules
- Notification visibility rules
