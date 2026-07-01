using MediatR;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Reports;

public record GetTransactionReportQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    Guid? CardId,
    string Format = "json"
) : IRequest<ApiResponse<List<TransactionReportDto>>>;

public record GetFraudReportQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    string Format = "json"
) : IRequest<ApiResponse<List<FraudReportDto>>>;

public record GetCardReportQuery(
    string? Status,
    string Format = "json"
) : IRequest<ApiResponse<List<CardReportDto>>>;

public record GetDailySummaryQuery(
    DateTime Date
) : IRequest<ApiResponse<DailySummaryDto>>;