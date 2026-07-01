using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(w => w.AvailableBalance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(w => w.Currency)
            .IsRequired()
            .HasMaxLength(3);

        // Optimistic concurrency token
        builder.Property(w => w.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}