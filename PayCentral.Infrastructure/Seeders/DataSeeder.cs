using Microsoft.EntityFrameworkCore;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;
using PayCentral.Infrastructure.Persistence;

namespace PayCentral.Infrastructure.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedMerchantsAsync(context);
        await SeedCardsAsync(context);
        await SeedTransactionsAsync(context);
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var users = new List<User>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@paycentral.co.za",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                PhoneNumber = "+27110000001",
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@paycentral.co.za",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cardholder@123", workFactor: 12),
                PhoneNumber = "+27110000002",
                Role = UserRole.Cardholder,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@paycentral.co.za",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cardholder@123", workFactor: 12),
                PhoneNumber = "+27110000003",
                Role = UserRole.Cardholder,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMerchantsAsync(AppDbContext context)
    {
        if (await context.Merchants.AnyAsync()) return;

        var merchants = new List<Merchant>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Name = "Checkers Sandton",
                Category = "Grocery",
                CountryCode = "ZA",
                City = "Johannesburg",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                Name = "Shell Garage Midrand",
                Category = "Fuel",
                CountryCode = "ZA",
                City = "Midrand",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                Name = "Amazon UK",
                Category = "Online Shopping",
                CountryCode = "GB",
                City = "London",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Merchants.AddRangeAsync(merchants);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCardsAsync(AppDbContext context)
    {
        if (await context.Cards.AnyAsync()) return;

        var cards = new List<Card>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                CardNumber = "4111111111111111",
                MaskedCardNumber = Card.MaskCardNumber("4111111111111111"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Status = CardStatus.Active,
                ExpiryDate = DateTime.UtcNow.AddYears(3),
                ActivatedAt = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Wallet = new Wallet
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                    Balance = 5000.00m,
                    AvailableBalance = 5000.00m,
                    Currency = "ZAR",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                CardNumber = "4222222222222222",
                MaskedCardNumber = Card.MaskCardNumber("4222222222222222"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Status = CardStatus.Active,
                ExpiryDate = DateTime.UtcNow.AddYears(3),
                ActivatedAt = DateTime.UtcNow.AddDays(-15),
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Wallet = new Wallet
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Balance = 12000.00m,
                    AvailableBalance = 12000.00m,
                    Currency = "ZAR",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            }
        };

        await context.Cards.AddRangeAsync(cards);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTransactionsAsync(AppDbContext context)
    {
        if (await context.Transactions.AnyAsync()) return;

        var transactions = new List<Transaction>
        {
            new()
            {
                ReferenceNumber = "TXN20240101000001",
                CardId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Type = TransactionType.LoadFunds,
                Status = TransactionStatus.Completed,
                Amount = 5000.00m,
                BalanceAfter = 5000.00m,
                Currency = "ZAR",
                Description = "Initial fund load",
                IsInternational = false,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                ReferenceNumber = "TXN20240102000001",
                CardId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 450.00m,
                BalanceAfter = 4550.00m,
                Currency = "ZAR",
                Description = "Grocery purchase",
                IsInternational = false,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                ReferenceNumber = "TXN20240103000001",
                CardId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 800.00m,
                BalanceAfter = 3750.00m,
                Currency = "ZAR",
                Description = "Fuel purchase",
                IsInternational = false,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                ReferenceNumber = "TXN20240104000001",
                CardId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 2500.00m,
                BalanceAfter = 1250.00m,
                Currency = "ZAR",
                Description = "Online purchase - International",
                IsInternational = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                ReferenceNumber = "TXN20240105000001",
                CardId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 21000.00m,
                BalanceAfter = 0.00m,
                Currency = "ZAR",
                Description = "Large purchase - fraud trigger",
                IsInternational = false,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
    }
}