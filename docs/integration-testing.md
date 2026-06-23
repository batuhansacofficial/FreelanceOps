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
  TestBillingHelper.cs
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
Auth -> Workspace -> Client -> Project -> Task -> TimeEntry -> Invoice -> Report
```

The suite currently contains 69 tests, including 16 billing tests and 18 reporting tests. Reporting coverage verifies authorization, tenant isolation, dashboard metrics, payment-based revenue, multi-currency grouping, date validation, client summaries, and project performance.

They intentionally do not attempt exhaustive CRUD coverage. Financial reports are tested through the public HTTP routes against PostgreSQL.
