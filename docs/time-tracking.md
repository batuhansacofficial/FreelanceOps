# Time Tracking

Time entries connect a workspace member to an active project task. Entries can originate from a running timer or manual input.

## Endpoints

```http
POST   /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/start
POST   /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/manual
POST   /api/workspaces/{workspaceId}/time-entries/{timeEntryId}/stop
GET    /api/workspaces/{workspaceId}/time-entries
GET    /api/workspaces/{workspaceId}/projects/{projectId}/time-entries
GET    /api/workspaces/{workspaceId}/tasks/{taskId}/time-entries
PUT    /api/workspaces/{workspaceId}/time-entries/{timeEntryId}
DELETE /api/workspaces/{workspaceId}/time-entries/{timeEntryId}
GET    /api/workspaces/{workspaceId}/reports/time-summary
```

## Timer Rules

- Any active workspace member can start a timer for an active task.
- A user can have only one active timer across all workspaces.
- Members can stop only their own timer.
- Owner/Admin users can stop any timer in their workspace.
- Timer duration is rounded up to the next whole minute when stopped.

## Manual Entries

Manual entries require:

```text
startedAtUtc <= current UTC time
durationMinutes between 1 and 1440
description length <= 2000
```

Only manual entries can be edited. Members can edit/delete their own entries; Owner/Admin users can manage any entry in the workspace.

## Visibility

```text
Owner/Admin: all non-deleted workspace entries
Member:      own non-deleted entries only
```

For members, a supplied `userId` filter is ignored and replaced with the authenticated user's id.

## Summary

```http
GET /api/workspaces/{workspaceId}/reports/time-summary?from=2026-06-01&to=2026-06-30&projectId={projectId}&taskId={taskId}
```

Running timers are excluded because they do not have a final duration. The response includes total minutes/hours, entry count, totals by project, and totals by user.
