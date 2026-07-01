using MediatR;

namespace PayCentral.Application.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;