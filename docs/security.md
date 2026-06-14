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

- Workspace membership checks
- Workspace role checks
- `403` behavior for workspace-scoped operations

Do not add client, project, task, invoice, or report modules before workspace authorization is reliable.
