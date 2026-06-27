using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.WorkspaceId)
            .IsRequired();

        builder.Property(project => project.ClientId)
            .IsRequired();

        builder.Property(project => project.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(4000);

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(project => project.StartDate);

        builder.Property(project => project.Deadline);

        builder.Property(project => project.CreatedAtUtc)
            .IsRequired();

        builder.Property(project => project.UpdatedAtUtc);

        builder.Property(project => project.IsDeleted)
            .IsRequired();

        builder.HasIndex(project => project.WorkspaceId);

        builder.HasIndex(project => project.ClientId);

        builder.HasIndex(project => new
        {
            project.WorkspaceId,
            project.Status
        });

        builder.HasIndex(project => new
        {
            project.WorkspaceId,
            project.Name
        });
    }
}
