using FreelanceOps.Domain.Common;

namespace FreelanceOps.Domain.Notifications;

public sealed class Notification
{
    private Notification()
    {
    }

    public Notification(
        Guid workspaceId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? deduplicationKey)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Notification title is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Notification message is required.");
        }

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        UserId = userId;
        Type = type;
        Title = title.Trim();
        Message = message.Trim();
        RelatedEntityType = NormalizeOptional(relatedEntityType);
        RelatedEntityId = relatedEntityId;
        DeduplicationKey = NormalizeOptional(deduplicationKey);
        IsRead = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? DeduplicationKey { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
