using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(workspace => workspace.Id);

        builder.Property(workspace => workspace.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(workspace => workspace.Slug)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(workspace => workspace.Slug)
            .IsUnique();

        builder.Property(workspace => workspace.OwnerUserId)
            .IsRequired();

        builder.Property(workspace => workspace.CreatedAtUtc)
            .IsRequired();

        builder.Property(workspace => workspace.IsDeleted)
            .IsRequired();

        builder.HasMany(workspace => workspace.Members)
            .WithOne()
            .HasForeignKey(member => member.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(workspace => workspace.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
