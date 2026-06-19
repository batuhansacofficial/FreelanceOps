using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.InvoiceId)
            .IsRequired();

        builder.Property(item => item.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.TaxRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(item => item.SubtotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(item => item.InvoiceId);
    }
}
