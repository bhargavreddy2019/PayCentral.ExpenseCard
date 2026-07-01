using MediatR;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Cards;

public record CreateCardCommand(Guid UserId)
    : IRequest<ApiResponse<CardDto>>;

public record ActivateCardCommand(Guid CardId)
    : IRequest<ApiResponse<CardDto>>;

public record BlockCardCommand(Guid CardId, string Reason)
    : IRequest<ApiResponse<CardDto>>;

public record UnblockCardCommand(Guid CardId)
    : IRequest<ApiResponse<CardDto>>;

public record SuspendCardCommand(Guid CardId, string Reason)
    : IRequest<ApiResponse<CardDto>>;

public record CloseCardCommand(Guid CardId, string Reason)
    : IRequest<ApiResponse<CardDto>>;