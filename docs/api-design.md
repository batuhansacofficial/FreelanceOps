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
