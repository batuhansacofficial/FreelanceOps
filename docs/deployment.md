# Deployment

This milestone prepares deployable artifacts. It does not publish images or
perform a live deployment.

## Deployables

- `FreelanceOps.Api`
- `FreelanceOps.Worker`

The API image uses the ASP.NET runtime. The Worker image uses the smaller .NET
runtime because it does not host HTTP endpoints.

## Required Environment Variables

API container:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__Database
ConnectionStrings__Redis
Jwt__Issuer
Jwt__Audience
Jwt__Secret
DemoSeed__Enabled=false
```

Worker container:

```text
DOTNET_ENVIRONMENT=Production
ConnectionStrings__Database
ConnectionStrings__Redis
BackgroundJobs__DueDateMonitoringIntervalMinutes=60
DemoSeed__Enabled=false
```

`ConnectionStrings__Redis` is listed as part of the production configuration
contract because Redis is part of the local infrastructure definition. The
current API and Worker code paths do not consume Redis yet.

`Jwt__Secret` must be a production secret with at least 32 characters. Do not
reuse the development value.

## Docker Build

Build the API image:

```bash
docker build -f docker/Dockerfile.api -t freelanceops-api:local .
```

Build the Worker image:

```bash
docker build -f docker/Dockerfile.worker -t freelanceops-worker:local .
```

The CI workflow also validates both image builds without pushing images to a
registry.

## Running the API Container Locally

Start local dependencies:

```bash
docker compose up -d
```

Run the API image against the local PostgreSQL container from Docker Desktop:

```bash
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__Database="Host=host.docker.internal;Port=5432;Database=freelanceops;Username=freelanceops;Password=freelanceops_dev_password" \
  -e ConnectionStrings__Redis="host.docker.internal:6379" \
  -e Jwt__Issuer="FreelanceOps" \
  -e Jwt__Audience="FreelanceOps.Api" \
  -e Jwt__Secret="change-this-production-secret-minimum-32-characters" \
  -e DemoSeed__Enabled=false \
  freelanceops-api:local
```

Check health:

```bash
curl http://localhost:8080/health
```

PowerShell alternative:

```powershell
Invoke-WebRequest http://localhost:8080/health
```

Expected response:

```text
Healthy
```

## Running the Worker Container Locally

Run the Worker image against the local PostgreSQL container from Docker
Desktop:

```bash
docker run --rm \
  -e DOTNET_ENVIRONMENT=Production \
  -e ConnectionStrings__Database="Host=host.docker.internal;Port=5432;Database=freelanceops;Username=freelanceops;Password=freelanceops_dev_password" \
  -e ConnectionStrings__Redis="host.docker.internal:6379" \
  -e BackgroundJobs__DueDateMonitoringIntervalMinutes=60 \
  -e DemoSeed__Enabled=false \
  freelanceops-worker:local
```

The Worker runs due-date monitoring on a fixed interval and writes hosted-service
logs to stdout.

## Database Migrations

Do not run migrations automatically from API or Worker startup in production.

Deployment flow:

```text
1. Build the new images.
2. Take a database backup.
3. Run migrations as a controlled deployment step.
4. Deploy the API image.
5. Deploy the Worker image.
6. Verify /health.
```

Migration command:

```bash
dotnet ef database update \
  --project src/FreelanceOps.Infrastructure \
  --startup-project src/FreelanceOps.Api
```

If the deployment environment does not include the .NET SDK, use a controlled
migration runner or evaluate `dotnet ef migrations bundle` in a later milestone.

## Demo Seed Warning

Production must set:

```text
DemoSeed__Enabled=false
```

The demo seeder is intended for local Development only. It must not create demo
users, demo workspace data, invoices, payments, or notifications in production
databases.

## Health Checks

The API exposes:

```text
GET /health
```

The health check includes the PostgreSQL-backed EF Core context. A healthy API
container should return:

```text
Healthy
```

## Known Limitations

- Docker images are built and verified, but not pushed to a registry yet.
- Live deployment is not part of this milestone.
- Database migrations are expected to run as a controlled deployment step.
- Demo seed is disabled in production environments.
- Worker scheduling uses a simple interval-based hosted service, not a distributed scheduler.
