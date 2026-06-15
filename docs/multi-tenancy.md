# Multi-Tenancy

FreelanceOps uses a single database and shared schema. Workspace ownership and membership enforce tenant boundaries.

## Tenant Boundary

Business data is scoped by `WorkspaceId`. The workspace module enforces:

- Active membership for workspace reads
- `Owner` or `Admin` role for member-management operations
- `Owner` role for workspace deletion

The client module enforces:

- Active workspace membership for client list and detail reads
- `Owner` or `Admin` role for client create, update, and delete
- `WorkspaceId` and `IsDeleted = false` filters for every client query
- `WorkspaceId`, `ClientId`, and `IsDeleted = false` filters for client detail, update, and delete lookups

## Roles

```text
Owner
Admin
Member
```

The `Client` role is intentionally not part of this milestone.

## Reusable Authorization Services

Application code should use:

```csharp
IWorkspaceAuthorizationService.EnsureMemberAsync(...)
IWorkspaceAuthorizationService.EnsureAnyRoleAsync(...)
```

Lower-level checks are available through:

```csharp
IWorkspaceAccessService.HasAccessAsync(...)
IWorkspaceAccessService.HasAnyRoleAsync(...)
IWorkspaceAccessService.GetRoleAsync(...)
```

Future modules such as projects, invoices, time tracking, and reports must call these services before returning or mutating workspace-scoped data.
