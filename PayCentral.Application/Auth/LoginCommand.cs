using Docker.DotNet.Models;
using MediatR;

namespace PayCentral.Application.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;