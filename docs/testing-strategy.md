# Testing Strategy

Initial test projects have not been added yet.

The first test scope should cover:

- Auth flow
- Workspace isolation
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
