# Security

The security model is being built in milestones.

Auth milestone:

- Custom JWT authentication
- Refresh token rotation
- Refresh token hashing before persistence
- Consistent `401` behavior for unauthenticated or invalid-token requests

Implemented:

- Register
- Login
- Access token generation
- Refresh token generation and rotation
- Logout with refresh token revocation
- Current user lookup through `/api/auth/me`

Still pending:

- Workspace-scoped guards for future clients, projects, invoices, and reports
- `403` behavior for future business modules

Do not add client, project, task, invoice, or report modules before workspace authorization is reliable.

## Workspace Authorization

Implemented workspace rules:

- Workspace creator becomes `Owner`.
- Active membership is required for workspace access.
- `Owner` and `Admin` can add members.
- `Owner` and `Admin` can change non-owner member roles.
- `Owner` and `Admin` can remove non-owner members.
- `Owner` cannot be removed.
- `Owner` role cannot be assigned through member endpoints.
- `Owner` role cannot be changed.

Reusable services:

- `IWorkspaceAccessService`
- `IWorkspaceAuthorizationService`
