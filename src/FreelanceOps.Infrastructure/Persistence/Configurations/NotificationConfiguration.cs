using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.WorkspaceId)
            .IsRequired();

        builder.Property(notification => notification.UserId)
            .IsRequired();

        builder.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(notification => notification.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(notification => notification.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(notification => notification.RelatedEntityType)
            .HasMaxLength(64);

        builder.Property(notification => notification.RelatedEntityId);

        builder.Property(notification => notification.DeduplicationKey)
            .HasMaxLength(200);

        builder.Property(notification => notification.IsRead)
            .IsRequired();

        builder.Property(notification => notification.CreatedAtUtc)
            .IsRequired();

        builder.Property(notification => notification.ReadAtUtc);

        builder.HasIndex(notification => new
        {
            notification.WorkspaceId,
            notification.UserId,
            notification.IsRead
        });

        builder.HasIndex(notification => new
        {
            notification.WorkspaceId,
            notification.UserId,
            notification.CreatedAtUtc
        });

        builder.HasIndex(notification => notification.DeduplicationKey)
            .IsUnique()
            .HasFilter("\"DeduplicationKey\" IS NOT NULL");
    }
}
