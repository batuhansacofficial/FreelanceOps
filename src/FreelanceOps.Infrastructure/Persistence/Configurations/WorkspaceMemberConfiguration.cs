using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("workspace_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.WorkspaceId)
            .IsRequired();

        builder.Property(member => member.UserId)
            .IsRequired();

        builder.Property(member => member.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(member => member.JoinedAtUtc)
            .IsRequired();

        builder.Property(member => member.IsActive)
            .IsRequired();

        builder.HasIndex(member => new
            {
                member.WorkspaceId,
                member.UserId
            })
            .IsUnique();

        builder.HasIndex(member => member.UserId);
        builder.HasIndex(member => member.WorkspaceId);
    }
}
