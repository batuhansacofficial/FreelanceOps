using FreelanceOps.Domain.TimeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("time_entries");

        builder.HasKey(timeEntry => timeEntry.Id);

        builder.Property(timeEntry => timeEntry.WorkspaceId)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.ProjectId)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.TaskId)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.UserId)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.StartedAtUtc)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.EndedAtUtc);

        builder.Property(timeEntry => timeEntry.DurationMinutes);

        builder.Property(timeEntry => timeEntry.Description)
            .HasMaxLength(2000);

        builder.Property(timeEntry => timeEntry.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.CreatedAtUtc)
            .IsRequired();

        builder.Property(timeEntry => timeEntry.UpdatedAtUtc);

        builder.Property(timeEntry => timeEntry.IsDeleted)
            .IsRequired();

        builder.Ignore(timeEntry => timeEntry.IsRunning);

        builder.HasIndex(timeEntry => timeEntry.WorkspaceId);

        builder.HasIndex(timeEntry => new
        {
            timeEntry.WorkspaceId,
            timeEntry.ProjectId
        });

        builder.HasIndex(timeEntry => new
        {
            timeEntry.WorkspaceId,
            timeEntry.TaskId
        });

        builder.HasIndex(timeEntry => new
        {
            timeEntry.WorkspaceId,
            timeEntry.UserId
        });

        builder.HasIndex(timeEntry => new
        {
            timeEntry.UserId,
            timeEntry.EndedAtUtc
        });
    }
}
