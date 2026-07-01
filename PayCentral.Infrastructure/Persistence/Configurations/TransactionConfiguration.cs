using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.ReferenceNumber)
            .IsUnique();

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.IdempotencyKey)
            .HasMaxLength(100);

        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        // Performance index — most common query pattern
        builder.HasIndex(t => new { t.CardId, t.CreatedAt });

        builder.HasOne(t => t.Merchant)
            .WithMany(m => m.Transactions)
            .HasForeignKey(t => t.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}