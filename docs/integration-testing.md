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
  TestProposalHelper.cs
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
Auth -> Workspace -> Client -> Proposal -> Project -> Task -> TimeEntry -> Invoice -> Report -> Notification -> BackgroundJob
```

The suite currently contains 96 tests, including 16 billing tests, 15 proposal tests, 18 reporting tests, 6 notification endpoint tests, and 6 background job tests. Proposal coverage verifies authorization, tenant isolation, totals, status transitions, expired acceptance rejection, soft delete, and convert-to-project rules. Reporting coverage verifies authorization, tenant isolation, dashboard metrics, payment-based revenue, multi-currency grouping, date validation, client summaries, and project performance. Notification coverage verifies per-user visibility, unread counts, read operations, expired proposal processing, and overdue invoice notifications.

They intentionally do not attempt exhaustive CRUD coverage. Financial reports are tested through the public HTTP routes against PostgreSQL.
