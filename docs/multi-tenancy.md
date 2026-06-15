# Multi-Tenancy

FreelanceOps uses a single database and shared schema. Workspace ownership and membership enforce tenant boundaries.

## Tenant Boundary

Business data should be scoped by `WorkspaceId`. For this milestone, the workspace module enforces:

- Active membership for workspace reads
- `Owner` or `Admin` role for member-management operations
- `Owner` role for workspace deletion

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

Future modules such as clients, projects, invoices, time tracking, and reports must call these services before returning or mutating workspace-scoped data.
