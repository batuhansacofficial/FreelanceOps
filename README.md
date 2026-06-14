# FreelanceOps

Production-style multi-tenant SaaS backend for freelancers and small agencies.

## Current Status

Initial solution setup is in place:

- ASP.NET Core Web API
- Application, Domain, Infrastructure, and Worker projects
- PostgreSQL EF Core configuration
- Docker Compose for PostgreSQL and Redis
- Health check endpoint at `/health`
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

## Known Limitations

- Auth is not implemented yet.
- Workspace isolation is not implemented yet.
- No database migrations exist yet because domain entities have not been added.
- Payment processing, email sending, and file storage are not implemented.
- Frontend is not part of the initial MVP.
