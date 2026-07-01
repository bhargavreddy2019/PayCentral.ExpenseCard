using Microsoft.AspNetCore.Http;
using PayCentral.Application.Common.Interfaces;
using System.Security.Claims;

namespace PayCentral.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);
            return value != null ? Guid.Parse(value) : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.Email);

    public string? Role =>
        _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.Role);

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?.ToString();
}