using Microsoft.EntityFrameworkCore;
using PayCentral.Domain.Entities;
using System.Collections.Generic;

namespace PayCentral.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Card> Cards { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<CardStatusHistory> CardStatusHistories { get; }
    DbSet<FraudAlert> FraudAlerts { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}