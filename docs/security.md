# Security

The first security milestone is:

- Custom JWT authentication
- Refresh token rotation
- Workspace membership checks
- Role checks for workspace-scoped operations
- Consistent `401` and `403` behavior

Do not add client, project, task, invoice, or report modules before workspace authorization is reliable.
