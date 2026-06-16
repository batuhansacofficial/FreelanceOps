# Testing Strategy

Initial test projects have not been added yet.

The first test scope should cover:

- Auth flow
- Workspace isolation
- Client authorization and tenant isolation
- Project and task tenant isolation
- Forbidden cross-workspace access
- Domain rules for invoices, proposals, and time tracking after those modules exist

Prioritize meaningful integration tests over high-volume shallow tests.

## Manual Auth Verification

Until integration test infrastructure is added, the auth flow has been manually verified against the real PostgreSQL-backed API:

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
