using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceOps.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.WorkspaceId)
            .IsRequired();

        builder.Property(invoice => invoice.ClientId)
            .IsRequired();

        builder.Property(invoice => invoice.ProjectId);

        builder.Property(invoice => invoice.InvoiceNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(invoice => invoice.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(invoice => invoice.IssueDate)
            .IsRequired();

        builder.Property(invoice => invoice.DueDate)
            .IsRequired();

        builder.Property(invoice => invoice.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(invoice => invoice.Notes)
            .HasMaxLength(2000);

        builder.Property(invoice => invoice.SubtotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.PaidAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.CreatedAtUtc)
            .IsRequired();

        builder.Property(invoice => invoice.UpdatedAtUtc);

        builder.Property(invoice => invoice.IsDeleted)
            .IsRequired();

        builder.Ignore(invoice => invoice.BalanceDue);

        builder.HasIndex(invoice => new
            {
                invoice.WorkspaceId,
                invoice.InvoiceNumber
            })
            .IsUnique();

        builder.HasIndex(invoice => new
        {
            invoice.WorkspaceId,
            invoice.Status
        });

        builder.HasIndex(invoice => new
        {
            invoice.WorkspaceId,
            invoice.ClientId
        });

        builder.HasIndex(invoice => new
        {
            invoice.WorkspaceId,
            invoice.ProjectId
        });

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(invoice => invoice.Payments)
            .WithOne()
            .HasForeignKey(payment => payment.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(invoice => invoice.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
