using MediatR;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Cards;

public record GetCardsQuery(
    string? SearchTerm,
    string? Status,
    int Page = 1,
    int PageSize = 10
) : IRequest<ApiResponse<List<CardDto>>>;

public record GetCardByIdQuery(Guid CardId)
    : IRequest<ApiResponse<CardDetailDto>>;

public record GetCardsByUserQuery(Guid UserId)
    : IRequest<ApiResponse<List<CardDto>>>;