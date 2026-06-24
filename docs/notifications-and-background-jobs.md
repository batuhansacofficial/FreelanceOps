# Notifications And Background Jobs

Notifications are persisted per workspace user. They are personal user data: a workspace `Owner` or `Admin` can see only their own notifications, not another member's notifications.

## Notification Model

Notification records include:

```text
WorkspaceId
UserId
Type
Title
Message
RelatedEntityType
RelatedEntityId
DeduplicationKey
IsRead
CreatedAtUtc
ReadAtUtc
```

Supported types:

```text
ProposalSent
ProposalAccepted
ProposalConvertedToProject
InvoiceSent
InvoicePaid
InvoiceOverdue
ProposalExpired
TaskAssigned
```

## Endpoints

```http
GET   /api/workspaces/{workspaceId}/notifications?page=1&pageSize=20&isRead=false
GET   /api/workspaces/{workspaceId}/notifications/unread-count
PATCH /api/workspaces/{workspaceId}/notifications/{notificationId}/read
PATCH /api/workspaces/{workspaceId}/notifications/read-all
```

Any active workspace member can use these endpoints for their own notifications. Non-members receive `403`. Notification lookups always include the current authenticated user id.

## Deduplication

Notifications can carry a nullable `DeduplicationKey`. PostgreSQL enforces uniqueness for non-null keys.

Background jobs use user-specific keys:

```text
proposal-expired:{proposalId}:{userId}
invoice-overdue:{invoiceId}:{userId}
```

Lifecycle notifications use the same pattern for manager fan-out:

```text
proposal-sent:{proposalId}:{userId}
proposal-accepted:{proposalId}:{userId}
proposal-converted:{proposalId}:{userId}
invoice-sent:{invoiceId}:{userId}
invoice-paid:{invoiceId}:{userId}
```

Task assignment uses:

```text
task-assigned:{taskId}:{userId}
```

## Expired Proposal Job

The expired proposal job finds:

```text
Proposal.Status == Sent
Proposal.ValidUntil < current UTC date
Proposal.IsDeleted == false
```

It changes the proposal status to `Expired` and creates `ProposalExpired` notifications for active workspace `Owner` and `Admin` users.

## Overdue Invoice Job

The overdue invoice job finds:

```text
Invoice.Status == Sent
Invoice.DueDate < current UTC date
Invoice.TotalAmount - Invoice.PaidAmount > 0
Invoice.IsDeleted == false
```

It creates `InvoiceOverdue` notifications for active workspace `Owner` and `Admin` users. It does not change invoice status. Overdue remains a computed billing/reporting state.

## Worker Interval

The Worker project runs both jobs through `DueDateMonitoringWorker`.

Configuration:

```json
{
  "BackgroundJobs": {
    "DueDateMonitoringIntervalMinutes": 60
  }
}
```

Development config uses a shorter interval. Integration tests call the job services directly instead of waiting for the hosted service loop.

## Known Limitations

- Notifications are persisted in the database.
- Real-time SignalR delivery is not implemented in this milestone.
- Email delivery is not implemented.
- Invoice overdue state is computed; the worker only creates notifications.
- Job scheduling uses a simple hosted service interval, not a distributed scheduler.
