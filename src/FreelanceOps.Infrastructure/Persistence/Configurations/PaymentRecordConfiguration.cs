using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable("payment_records");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.InvoiceId)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.Method)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(payment => payment.Reference)
            .HasMaxLength(200);

        builder.Property(payment => payment.PaidAt)
            .IsRequired();

        builder.Property(payment => payment.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(payment => payment.InvoiceId);
    }
}
