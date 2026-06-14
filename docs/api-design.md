# API Design

The first stable endpoints are:

```http
GET /health
GET /openapi/v1.json
GET /swagger
```

Business APIs should be workspace-scoped after auth and tenant isolation are implemented:

```http
GET /api/workspaces/{workspaceId}/clients
POST /api/workspaces/{workspaceId}/clients
GET /api/workspaces/{workspaceId}/projects
POST /api/workspaces/{workspaceId}/projects
```
