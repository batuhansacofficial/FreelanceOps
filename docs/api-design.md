# API Design

The first stable endpoints are:

```http
GET /health
GET /openapi/v1.json
GET /swagger
```

Auth endpoints:

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

Workspace endpoints:

```http
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

Every workspace endpoint requires a valid bearer token. Endpoints with a `workspaceId` route value also check active membership. Member-management endpoints require `Owner` or `Admin`, except workspace deletion, which requires `Owner`.

Client endpoints:

```http
POST   /api/workspaces/{workspaceId}/clients
GET    /api/workspaces/{workspaceId}/clients?page=1&pageSize=20&search=acme
GET    /api/workspaces/{workspaceId}/clients/{clientId}
PUT    /api/workspaces/{workspaceId}/clients/{clientId}
DELETE /api/workspaces/{workspaceId}/clients/{clientId}
```

Client list responses include:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

Client read endpoints require `Owner`, `Admin`, or `Member`. Client create, update, and delete endpoints require `Owner` or `Admin`.

Every client query filters by `WorkspaceId` and `IsDeleted = false`. Client detail, update, and delete lookups also filter by `ClientId`, so a known client id cannot be read through another workspace route.

Future business APIs should keep the same workspace route shape:

```http
GET  /api/workspaces/{workspaceId}/projects
POST /api/workspaces/{workspaceId}/projects
```
