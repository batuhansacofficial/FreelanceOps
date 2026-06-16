# Project And Task Management

Projects belong to a workspace and must be linked to a non-deleted client in that same workspace. Project tasks belong to a project and workspace.

## Project Endpoints

```http
POST   /api/workspaces/{workspaceId}/projects
GET    /api/workspaces/{workspaceId}/projects?page=1&pageSize=20&search=api&status=Active&clientId={clientId}
GET    /api/workspaces/{workspaceId}/projects/{projectId}
PUT    /api/workspaces/{workspaceId}/projects/{projectId}
PATCH  /api/workspaces/{workspaceId}/projects/{projectId}/status
DELETE /api/workspaces/{workspaceId}/projects/{projectId}
```

## Task Endpoints

```http
POST   /api/workspaces/{workspaceId}/projects/{projectId}/tasks
GET    /api/workspaces/{workspaceId}/projects/{projectId}/tasks?page=1&pageSize=20&status=Todo&assignedToUserId={userId}
GET    /api/workspaces/{workspaceId}/tasks/{taskId}
PUT    /api/workspaces/{workspaceId}/tasks/{taskId}
PATCH  /api/workspaces/{workspaceId}/tasks/{taskId}/status
DELETE /api/workspaces/{workspaceId}/tasks/{taskId}
```

## Permissions

```text
Project list/detail:          Owner, Admin, Member
Project create/update/status: Owner, Admin
Project delete:               Owner, Admin

Task list/detail:             Owner, Admin, Member
Task create/update/status:    Owner, Admin, Member
Task delete:                  Owner, Admin
```

## Tenant Rules

Project creation validates:

```csharp
client.Id == clientId &&
client.WorkspaceId == workspaceId &&
!client.IsDeleted
```

Task creation and listing validate:

```csharp
project.Id == projectId &&
project.WorkspaceId == workspaceId &&
!project.IsDeleted
```

Task detail, update, status change, and delete validate:

```csharp
task.Id == taskId &&
task.WorkspaceId == workspaceId &&
!task.IsDeleted
```

If `AssignedToUserId` is provided, it must match an active workspace member:

```csharp
member.WorkspaceId == workspaceId &&
member.UserId == assignedToUserId &&
member.IsActive
```

## Filters

Project lists support `page`, `pageSize`, `search`, `status`, and `clientId`.

Task lists support `page`, `pageSize`, `status`, and `assignedToUserId`.

Both list endpoints return `PagedResult<T>` metadata: `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, and `hasPreviousPage`.
