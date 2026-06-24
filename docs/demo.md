# Local Demo

FreelanceOps includes a local-only demo seed for Development runs.

The seed is disabled by default in `appsettings.json` and enabled in
`src/FreelanceOps.Api/appsettings.Development.json`:

```json
{
  "DemoSeed": {
    "Enabled": true,
    "ResetBeforeSeed": false
  }
}
```

The hosted seeder does not run outside the Development environment.

## Run Locally

Start PostgreSQL and Redis:

```bash
docker compose up -d
```

Restore the local EF tool and apply migrations:

```bash
dotnet tool restore
dotnet ef database update --project src/FreelanceOps.Infrastructure --startup-project src/FreelanceOps.Api
```

Run the API:

```bash
dotnet run --project src/FreelanceOps.Api/FreelanceOps.Api.csproj
```

The API listens on `http://localhost:5244` by default when using the HTTP
launch profile.

## Demo Credentials

```text
Email: demo@freelanceops.dev
Password: Demo123!
```

## Seeded Data

The demo seed creates:

- one demo user
- one demo workspace
- two clients
- three projects
- ten tasks
- manual time entries
- two proposals
- two invoices
- two payments
- unread notifications

The seeded data is designed to make dashboard, revenue, client summary, project
performance, invoice, time-entry, and notification endpoints return non-empty
results.

## Suggested API Flow

Use `docs/http/freelanceops-demo.http` with Visual Studio, Rider, or the VS Code
REST Client extension.

Recommended flow:

1. Login with the demo credentials.
2. Get the demo workspace.
3. Call dashboard and report endpoints.
4. Inspect clients, projects, invoices, and notifications.

## Resetting Demo Data

The seeder is idempotent. Re-running the API with the same database will not
duplicate demo records.

To recreate the demo dataset, temporarily set:

```json
{
  "DemoSeed": {
    "Enabled": true,
    "ResetBeforeSeed": true
  }
}
```

Run the API once, then set `ResetBeforeSeed` back to `false`.

## Known Limitations

- The demo seed is intended only for local Development.
- There is no public demo deployment in this milestone.
- The HTTP collection assumes the default local API URL.
