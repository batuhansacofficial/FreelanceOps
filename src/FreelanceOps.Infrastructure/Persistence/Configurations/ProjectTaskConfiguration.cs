using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("project_tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.WorkspaceId)
            .IsRequired();

        builder.Property(task => task.ProjectId)
            .IsRequired();

        builder.Property(task => task.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(task => task.Description)
            .HasMaxLength(4000);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(task => task.Priority)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(task => task.DueDate);

        builder.Property(task => task.AssignedToUserId);

        builder.Property(task => task.CreatedAtUtc)
            .IsRequired();

        builder.Property(task => task.UpdatedAtUtc);

        builder.Property(task => task.IsDeleted)
            .IsRequired();

        builder.HasIndex(task => task.WorkspaceId);

        builder.HasIndex(task => task.ProjectId);

        builder.HasIndex(task => task.AssignedToUserId);

        builder.HasIndex(task => new
        {
            task.WorkspaceId,
            task.Status
        });

        builder.HasIndex(task => new
        {
            task.WorkspaceId,
            task.ProjectId,
            task.Status
        });
    }
}
