# Testing Strategy

Integration tests live in:

```text
tests/FreelanceOps.IntegrationTests
```

The integration suite uses:

```text
xUnit
Microsoft.AspNetCore.Mvc.Testing
PostgreSQL Testcontainers
FluentAssertions
```

Run locally:

```bash
dotnet test FreelanceOps.sln --configuration Release
```

The test host starts the API through `WebApplicationFactory<Program>`, starts an isolated PostgreSQL container, overrides the database/JWT configuration for the `Testing` environment, and applies EF Core migrations before tests run.

The first test scope should cover:

- Auth flow
- Workspace isolation
- Client authorization and tenant isolation
- Project and task tenant isolation
- Time tracking authorization, timer state, and visibility rules
- Billing authorization, tenant isolation, totals, and lifecycle rules
- Forbidden cross-workspace access
- Domain rules for invoices and proposals after those modules exist

Prioritize meaningful integration tests over high-volume shallow tests.

## Automated Integration Coverage

Current integration tests cover:

- Auth registration, duplicate email conflict, login token response, missing-token authorization, and refresh-token reuse rejection
- Workspace owner membership creation and non-member/member authorization boundaries
- Client create authorization, cross-workspace detail protection, and soft-delete list filtering
- Project create authorization, client workspace validation, and cross-workspace detail protection
- Task assignment validation, cross-workspace project protection, member task status changes, and member project-status denial
- Time tracking start/stop behavior, global active-timer conflicts, task/workspace validation, manual duration validation, role-based entry visibility, summaries, and soft delete
- Billing manager-only access, client/project integrity, invoice totals/numbers, state transitions, payments, workspace lists, updates, and soft delete
- Proposal manager-only access, client integrity, proposal totals/numbers, state transitions, expired acceptance rejection, workspace lists, soft delete, and project conversion
- Reporting manager-only access, empty-state metrics, payment-based revenue, outstanding/overdue invoices, stopped-time metrics, date validation, multi-currency grouping, and tenant isolation
- Notification member access, per-user visibility, unread counts, read/read-all behavior, and background job deduplication

CI runs:

```bash
dotnet restore FreelanceOps.sln
dotnet build FreelanceOps.sln --no-restore --configuration Release
dotnet test FreelanceOps.sln --no-build --configuration Release
```

## Manual Auth Verification

The auth flow has also been manually verified against the real PostgreSQL-backed API:

- Missing token on `/api/auth/me` returns `401`
- Register returns `201`
- Duplicate email returns `409`
- Login returns access and refresh tokens
- `/api/auth/me` returns the active user with a valid bearer token
- Refresh token rotation returns a new access token and refresh token
- Reusing the old refresh token returns `401`
- Logout revokes the active refresh token
- Refresh after logout returns `401`
- Invalid bearer token on `/api/auth/me` returns `401`

## Manual Workspace Verification

The workspace flow has been manually verified against the real PostgreSQL-backed API:

- Missing token on `/api/workspaces` returns `401`
- User A creates workspace and receives `Owner`
- User A sees the workspace in `/api/workspaces`
- User B cannot access User A workspace before membership and receives `403`
- User A adds User B as `Member`
- Duplicate active membership returns `409`
- User B can access the workspace after membership
- User B cannot add another member while only `Member`
- User A changes User B role to `Admin`
- User B can add another registered user after becoming `Admin`
- Removing the `Owner` member returns `400`
- Demoting the `Owner` member returns `400`
- Unknown workspace id returns `404`

## Manual Client Verification

The client flow has been manually verified against the real PostgreSQL-backed API:

- User A and User B can register and login
- User A creates a workspace
- User A creates a client and receives `201`
- User A lists clients and sees the created client
- User A reads client detail and receives `200`
- User B cannot list User A workspace clients before membership and receives `403`
- User A adds User B as `Member`
- User B can list clients after membership and receives `200`
- User B cannot create clients while only `Member` and receives `403`
- User A promotes User B to `Admin`
- User B can create a client after becoming `Admin`
- Client list pagination returns correct page metadata
- Client list search returns the matching workspace client
- User A updates a client and receives `204`
- User A soft-deletes a client and receives `204`
- Deleted clients are hidden from list responses
- Deleted client detail returns `404`
- Looking up a client id through another workspace route returns `404`

## Manual Project And Task Verification

The project/task flow has been manually verified against the real PostgreSQL-backed API:

- User A and User B can register and login
- User A creates a workspace and client
- User A creates a project and receives `201`
- Project list includes the created project
- Project search, status filter, client filter, and pagination return correct metadata/results
- User B cannot list projects before membership and receives `403`
- User A adds User B as `Member`
- User B can list projects after membership
- User B cannot create projects while only `Member`
- User A creates a task assigned to User B
- User B can create a task as a workspace member
- User B can change task status
- User B cannot change project status
- User A can change project status
- Task list status and assignee filters return the expected task
- User B can read task detail and update task fields
- User B cannot delete a task
- User A can delete a task
- Deleted task detail returns `404`
- User A can update a project
- Project creation with a client from another workspace returns `404`
- Task creation with a project id from another workspace returns `404`
- User A can delete a project
- Deleted project detail returns `404`
- Task create/update rejects non-member `AssignedToUserId` values with `404`

## Manual Time Tracking Verification

The time-tracking flow has been manually verified against the real PostgreSQL-backed API:

- User A starts a timer and receives `201`
- Starting a second active timer for the same user returns `409`
- User A stops the running timer and receives `204`
- User A creates a manual time entry and receives `201`
- A non-member cannot list workspace time entries and receives `403`
- User B can start a timer after becoming a workspace member
- Owner listing includes User B's time entry
- Member listing is forced to User B's own entries even when another `userId` is requested
- Time summary includes stopped timers and manual entries
- Starting a timer through another workspace route returns `404`
- Starting a timer for a deleted task returns `404`
- User B can stop their own timer

## Manual Billing Verification

The billing flow has been manually verified against the real PostgreSQL-backed API:

- User A creates an invoice and receives `201`
- Invoice item totals are calculated correctly
- Non-members and members receive `403` from invoice lists
- User A sends a draft invoice and receives `204`
- Recording a full payment returns `201` and changes the invoice to `Paid`
- Cancelling a paid invoice returns `400`
- Cross-workspace clients and projects return `404`
- A project/client mismatch returns `400`

## Proposal Verification

Proposal integration tests verify:

- Owner and Admin-only endpoint access
- Member `403` responses
- Cross-workspace client rejection
- Item-based subtotal, tax, and total calculations
- Workspace/year proposal number generation
- Workspace-isolated lists
- Draft-only update and soft-delete rules
- Draft to sent to accepted/rejected status changes
- Expired proposal acceptance rejection
- Accepted proposal conversion to one draft project

## Notification And Background Job Verification

Notification and background job integration tests verify:

- Current users only see their own workspace notifications
- Non-members receive `403`
- Unread count ignores read notifications and other users' notifications
- Single notification read and read-all operations
- Another user's notification returns `404` when marked as read
- Expired proposal job changes `Sent` proposals to `Expired`
- Expired proposal job notifies Owner/Admin users without duplicates
- Overdue invoice job creates notifications without changing invoice status
- Overdue invoice job does not duplicate notifications

## Reporting Verification

Reporting integration tests verify:

- Owner and Admin access
- Member and non-member `403` responses
- Empty dashboard metrics
- Active/completed project and client counts
- Payment-based revenue instead of invoice totals
- Outstanding and overdue invoice calculations
- Active-timer exclusion
- Multi-currency revenue grouping
- Date range and grouping validation
- Client and project metrics
- Cross-workspace isolation
