using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;

namespace BudgetManager.Service.Extensions;

/// <summary>
/// Extension methods for configuring user context services.
/// </summary>
public static class UserContextServiceExtensions
{
    /// <summary>
    /// Adds user context services to the service collection.
    /// This includes HttpContextAccessor and ICurrentUserService for accessing
    /// the authenticated user's information from HTTP requests.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        // Register HTTP context accessor (required for ICurrentUserService)
        services.AddHttpContextAccessor();

        // Register user context service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register account balance service
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();

        return services;
    }
}
