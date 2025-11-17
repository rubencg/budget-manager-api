using System.Security.Claims;

namespace BudgetManager.Service.Services.UserContext;

/// <inheritdoc />
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User
                                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? _httpContextAccessor.HttpContext?.User
                                ?.FindFirst("sub")?.Value  // Auth0 uses 'sub' claim
                            ?? throw new UnauthorizedAccessException("User ID not found in token");

    public string? Email => _httpContextAccessor.HttpContext?.User
        ?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
