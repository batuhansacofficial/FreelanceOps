# FreelanceOps

Production-style multi-tenant SaaS backend for freelancers and small agencies.

## Current Status

Initial solution setup is in place:

- ASP.NET Core Web API
- Application, Domain, Infrastructure, and Worker projects
- PostgreSQL EF Core configuration
- Docker Compose for PostgreSQL and Redis
- Health check endpoint at `/health`
- Auth endpoints for register, login, refresh-token rotation, logout, and current user
- Workspace and workspace-member endpoints with role-based access checks
- OpenAPI document and Swagger UI in development
- Global exception middleware

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- Docker Compose

## Architecture

The project uses a modular monolith with vertical slices planned inside the application layer. The initial dependency direction is:

```text
Api -> Application
Api -> Infrastructure
Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
Worker -> Application
Worker -> Infrastructure
```

`Domain` has no project dependencies.

## Local Setup

Start infrastructure:

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project src/FreelanceOps.Api/FreelanceOps.Api.csproj
```

Swagger UI is available in development at:

```text
/swagger
```

Health check:

```text
/health
```

Auth endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

Workspace endpoints:

```text
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

## Known Limitations

- Workspace isolation exists for workspace and member endpoints only; future business entities still need workspace-scoped guards.
- Client, project, invoice, and report modules are not implemented yet.
- Automated integration tests are not implemented yet.
- Payment processing, email sending, and file storage are not implemented.
- Frontend is not part of the initial MVP.
