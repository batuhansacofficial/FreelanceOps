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

Business APIs should be workspace-scoped after auth and tenant isolation are implemented:

```http
GET /api/workspaces/{workspaceId}/clients
POST /api/workspaces/{workspaceId}/clients
GET /api/workspaces/{workspaceId}/projects
POST /api/workspaces/{workspaceId}/projects
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
