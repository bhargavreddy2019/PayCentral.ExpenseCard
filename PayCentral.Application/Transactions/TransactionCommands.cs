using MediatR;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Transactions;

public record LoadFundsCommand(
    Guid CardId,
    decimal Amount,
    string? Description,
    string? IdempotencyKey
) : IRequest<ApiResponse<TransactionDto>>;

public record PurchaseCommand(
    Guid CardId,
    decimal Amount,
    Guid? MerchantId,
    string? Description,
    bool IsInternational,
    string? IdempotencyKey
) : IRequest<ApiResponse<TransactionDto>>;

public record RefundCommand(
    Guid CardId,
    string OriginalReferenceNumber,
    decimal Amount,
    string? Description,
    string? IdempotencyKey
) : IRequest<ApiResponse<TransactionDto>>;

public record ReversalCommand(
    string OriginalReferenceNumber,
    string? IdempotencyKey
) : IRequest<ApiResponse<TransactionDto>>;

public record GetBalanceQuery(Guid CardId)
    : IRequest<ApiResponse<WalletDto>>;

public record GetTransactionsQuery(
    Guid? CardId,
    string? ReferenceNumber,
    string? MerchantName,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 10
) : IRequest<ApiResponse<List<TransactionDto>>>;