using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Persistence.Configurations;

public class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
{
    public void Configure(EntityTypeBuilder<FraudAlert> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.AlertType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.Severity)
            .IsRequired();

        // Performance index for admin dashboard
        builder.HasIndex(f => new { f.Severity, f.CreatedAt });
        builder.HasIndex(f => f.IsResolved);
    }
}