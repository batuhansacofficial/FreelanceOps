# Client Management

Clients are workspace-scoped business records. They represent a freelancer's customer, not a user account in the system.

## Endpoints

```http
POST   /api/workspaces/{workspaceId}/clients
GET    /api/workspaces/{workspaceId}/clients?page=1&pageSize=20&search=acme
GET    /api/workspaces/{workspaceId}/clients/{clientId}
PUT    /api/workspaces/{workspaceId}/clients/{clientId}
DELETE /api/workspaces/{workspaceId}/clients/{clientId}
```

## Permissions

```text
Owner/Admin: create, list, detail, update, delete
Member:      list, detail
Non-member:  forbidden
```

## Tenant Rules

Every client read or write is scoped to the route workspace:

```csharp
client.WorkspaceId == workspaceId && !client.IsDeleted
```

Detail, update, and delete operations also require the route client id:

```csharp
client.Id == clientId
```

This prevents a client id from one workspace being resolved through another workspace route.

## List Behavior

Client lists support:

```text
page >= 1
pageSize between 1 and 100
search length <= 100
```

Search currently matches `Name`, `Email`, and `CompanyName`. Results are ordered by `CreatedAtUtc` descending, then `Name`.

List responses include `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, and `hasPreviousPage`.
