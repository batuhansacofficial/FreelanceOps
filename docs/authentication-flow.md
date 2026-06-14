# Authentication Flow

FreelanceOps uses custom JWT authentication with refresh-token rotation.

## Register

```http
POST /api/auth/register
```

Register creates a user and does not return tokens. The expected flow is:

```text
Register -> Login -> Access token + refresh token
```

## Login

```http
POST /api/auth/login
```

Successful login returns:

- Access token
- Refresh token
- Access token expiration timestamp
- Basic user identity

Refresh tokens are returned as plain text once. Only their SHA-256 hash is stored in PostgreSQL.

## Refresh Token Rotation

```http
POST /api/auth/refresh-token
```

A valid refresh token:

- Is matched by hash
- Must not be revoked
- Must not be expired
- Revokes the old refresh token
- Issues a new access token and refresh token

Reusing an old refresh token returns `401`.

## Logout

```http
POST /api/auth/logout
```

Logout requires a valid access token and the active refresh token. The matching refresh token is revoked.

## Current User

```http
GET /api/auth/me
```

The endpoint requires a valid access token and returns the active user.
