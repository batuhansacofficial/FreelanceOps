using FreelanceOps.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(client => client.Id);

        builder.Property(client => client.WorkspaceId)
            .IsRequired();

        builder.Property(client => client.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(client => client.Email)
            .HasMaxLength(320);

        builder.Property(client => client.CompanyName)
            .HasMaxLength(160);

        builder.Property(client => client.Notes)
            .HasMaxLength(2000);

        builder.Property(client => client.CreatedAtUtc)
            .IsRequired();

        builder.Property(client => client.UpdatedAtUtc);

        builder.Property(client => client.IsDeleted)
            .IsRequired();

        builder.HasIndex(client => client.WorkspaceId);

        builder.HasIndex(client => new
        {
            client.WorkspaceId,
            client.Email
        });

        builder.HasIndex(client => new
        {
            client.WorkspaceId,
            client.Name
        });
    }
}
