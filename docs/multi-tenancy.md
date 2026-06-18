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

The project module enforces:

- Active workspace membership for project list and detail reads
- `Owner` or `Admin` role for project create, update, status change, and delete
- Same-workspace client validation before project creation
- `WorkspaceId`, `ProjectId`, and `IsDeleted = false` filters for project detail, update, status change, and delete lookups

The task module enforces:

- Active workspace membership for task list, detail, create, update, and status change
- `Owner` or `Admin` role for task delete
- Same-workspace project validation before task creation and task listing
- Active workspace membership validation for `AssignedToUserId`
- `WorkspaceId`, `TaskId`, and `IsDeleted = false` filters for task detail, update, status change, and delete lookups

The time-tracking module enforces:

- Active workspace membership before timer, manual-entry, list, update, delete, and summary operations
- Same-workspace active task and project validation before creating time entries
- A global active-timer check by `UserId`, independent of workspace
- `Owner` or `Admin` visibility across all workspace entries
- Member visibility restricted to the current user's entries, even when another `userId` query value is supplied
- Members may stop, update, or delete only their own permitted entries
- `WorkspaceId`, `TimeEntryId`, and `IsDeleted = false` filters for entry mutations

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

Future modules such as invoices and broader reports must call these services before returning or mutating workspace-scoped data.
