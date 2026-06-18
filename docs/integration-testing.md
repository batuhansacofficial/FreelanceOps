# Integration Testing

The integration test project is:

```text
tests/FreelanceOps.IntegrationTests
```

It starts the API in memory with `WebApplicationFactory<Program>` and uses a PostgreSQL Testcontainer as the backing database. EF Core migrations run automatically before tests execute.

## Stack

```text
xUnit
Microsoft.AspNetCore.Mvc.Testing
Testcontainers.PostgreSql
FluentAssertions
```

## Running Tests

Docker must be available because the tests start PostgreSQL containers.

```bash
dotnet test FreelanceOps.sln --configuration Release
```

The suite disables test parallelization to avoid multiple test classes mutating process-level configuration at the same time.

## Test Infrastructure

```text
Infrastructure/
  CustomWebApplicationFactory.cs
  IntegrationTestBase.cs
  TestAuthHelper.cs
  TestWorkspaceHelper.cs
  TestClientHelper.cs
  TestProjectHelper.cs
  TestTimeEntryHelper.cs
```

`CustomWebApplicationFactory`:

- Starts `postgres:17-alpine`
- Overrides `ConnectionStrings__Database`
- Overrides JWT settings for the `Testing` environment
- Replaces the API `ApplicationDbContext` registration
- Applies EF Core migrations

## Coverage Focus

The tests prioritize tenant isolation and authorization across:

```text
Auth -> Workspace -> Client -> Project -> Task -> TimeEntry
```

The suite currently contains 35 tests, including 14 time-tracking tests for timer conflicts, ownership, visibility, updates, summaries, and soft delete.

They intentionally do not attempt exhaustive CRUD coverage. Future billing, invoicing, and reporting modules should add tests for money calculations, state transitions, and cross-workspace access before implementation is accepted.
