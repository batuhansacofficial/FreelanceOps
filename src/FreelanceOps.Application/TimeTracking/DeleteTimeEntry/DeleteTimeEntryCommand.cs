namespace FreelanceOps.Application.TimeTracking.DeleteTimeEntry;

public sealed record DeleteTimeEntryCommand(
    Guid WorkspaceId,
    Guid TimeEntryId);
