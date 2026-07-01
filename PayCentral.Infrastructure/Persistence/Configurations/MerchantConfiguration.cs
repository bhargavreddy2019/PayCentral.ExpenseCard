using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Persistence.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.CountryCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(m => m.City)
            .HasMaxLength(100);
    }
}