using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        builder.Property(c => c.MaskedCardNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.BlockReason)
            .HasMaxLength(500);

        // One card has one wallet
        builder.HasOne(c => c.Wallet)
            .WithOne(w => w.Card)
            .HasForeignKey<Wallet>(w => w.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        // One card has many transactions
        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Card)
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        // One card has many status history entries
        builder.HasMany(c => c.StatusHistory)
            .WithOne(s => s.Card)
            .HasForeignKey(s => s.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        // One card has many fraud alerts
        builder.HasMany(c => c.FraudAlerts)
            .WithOne(f => f.Card)
            .HasForeignKey(f => f.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}