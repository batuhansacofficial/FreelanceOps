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
20260616180417_AddProjectTables
20260618133241_AddTimeTrackingTables
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

Project table:

```text
freelance_ops.projects
```

Columns:

```text
Id uuid primary key
WorkspaceId uuid not null
ClientId uuid not null
Name varchar(160) not null
Description varchar(4000) null
Status varchar(32) not null
StartDate date null
Deadline date null
CreatedAtUtc timestamptz not null
UpdatedAtUtc timestamptz null
IsDeleted boolean not null
```

Indexes:

```text
IX_projects_ClientId
IX_projects_WorkspaceId
IX_projects_WorkspaceId_Name
IX_projects_WorkspaceId_Status
```

Project queries must include `WorkspaceId` and `IsDeleted = false`. Project creation must validate that `ClientId` belongs to the same workspace and is not deleted.

Project task table:

```text
freelance_ops.project_tasks
```

Columns:

```text
Id uuid primary key
WorkspaceId uuid not null
ProjectId uuid not null
Title varchar(200) not null
Description varchar(4000) null
Status varchar(32) not null
Priority varchar(32) not null
DueDate date null
AssignedToUserId uuid null
CreatedAtUtc timestamptz not null
UpdatedAtUtc timestamptz null
IsDeleted boolean not null
```

Indexes:

```text
IX_project_tasks_AssignedToUserId
IX_project_tasks_ProjectId
IX_project_tasks_WorkspaceId
IX_project_tasks_WorkspaceId_ProjectId_Status
IX_project_tasks_WorkspaceId_Status
```

Task queries must include `WorkspaceId` and `IsDeleted = false`. Task creation must validate that `ProjectId` belongs to the same workspace and is not deleted. Task assignment must validate that `AssignedToUserId`, when provided, is an active member of the same workspace.

Time entry table:

```text
freelance_ops.time_entries
```

Columns:

```text
Id uuid primary key
WorkspaceId uuid not null
ProjectId uuid not null
TaskId uuid not null
UserId uuid not null
StartedAtUtc timestamptz not null
EndedAtUtc timestamptz null
DurationMinutes integer null
Description varchar(2000) null
Source varchar(32) not null
CreatedAtUtc timestamptz not null
UpdatedAtUtc timestamptz null
IsDeleted boolean not null
```

Indexes:

```text
IX_time_entries_UserId_EndedAtUtc
IX_time_entries_WorkspaceId
IX_time_entries_WorkspaceId_ProjectId
IX_time_entries_WorkspaceId_TaskId
IX_time_entries_WorkspaceId_UserId
```

Time-entry lookups include `WorkspaceId` and `IsDeleted = false`. Timer/manual creation validates that the task and parent project are active members of the same workspace.
