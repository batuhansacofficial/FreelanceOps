using FreelanceOps.Domain.Proposals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("proposals");

        builder.HasKey(proposal => proposal.Id);

        builder.Property(proposal => proposal.WorkspaceId)
            .IsRequired();

        builder.Property(proposal => proposal.ClientId)
            .IsRequired();

        builder.Property(proposal => proposal.ConvertedProjectId);

        builder.Property(proposal => proposal.ProposalNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(proposal => proposal.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(proposal => proposal.Scope)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(proposal => proposal.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(proposal => proposal.ValidUntil)
            .IsRequired();

        builder.Property(proposal => proposal.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(proposal => proposal.SubtotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(proposal => proposal.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(proposal => proposal.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(proposal => proposal.CreatedAtUtc)
            .IsRequired();

        builder.Property(proposal => proposal.UpdatedAtUtc);

        builder.Property(proposal => proposal.IsDeleted)
            .IsRequired();

        builder.HasIndex(proposal => new
            {
                proposal.WorkspaceId,
                proposal.ProposalNumber
            })
            .IsUnique();

        builder.HasIndex(proposal => new
        {
            proposal.WorkspaceId,
            proposal.Status
        });

        builder.HasIndex(proposal => new
        {
            proposal.WorkspaceId,
            proposal.ClientId
        });

        builder.HasIndex(proposal => new
        {
            proposal.WorkspaceId,
            proposal.Title
        });

        builder.HasIndex(proposal => proposal.ConvertedProjectId)
            .IsUnique()
            .HasFilter("\"ConvertedProjectId\" IS NOT NULL");

        builder.HasMany(proposal => proposal.Items)
            .WithOne()
            .HasForeignKey(item => item.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(proposal => proposal.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
