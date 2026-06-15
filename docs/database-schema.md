# Database Schema

The EF Core `ApplicationDbContext` is configured for PostgreSQL and uses the default schema:

```text
freelance_ops
```

Current migrations:

```text
20260614221112_AddIdentityTables
20260615181616_AddWorkspaceTables
20260615185834_AddClientTables
```

Client table:

```text
freelance_ops.clients
```

Columns:

```text
Id uuid primary key
WorkspaceId uuid not null
Name varchar(160) not null
Email varchar(320) null
CompanyName varchar(160) null
Notes varchar(2000) null
CreatedAtUtc timestamptz not null
UpdatedAtUtc timestamptz null
IsDeleted boolean not null
```

Indexes:

```text
IX_clients_WorkspaceId
IX_clients_WorkspaceId_Email
IX_clients_WorkspaceId_Name
```

Client queries must include `WorkspaceId` and `IsDeleted = false` unless the operation is an explicit administrative data-recovery task.
