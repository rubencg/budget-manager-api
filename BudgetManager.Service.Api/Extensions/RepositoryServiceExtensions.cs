using BudgetManager.Service.Infrastructure.Cosmos.Repositories;

namespace BudgetManager.Service.Extensions;

/// <summary>
/// Extension methods for registering repository services.
/// </summary>
public static class RepositoryServiceExtensions
{
    /// <summary>
    /// Adds all repository services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPlannedExpenseRepository, PlannedExpenseRepository>();
        services.AddScoped<ISavingRepository, SavingRepository>();
        services.AddScoped<IMonthlyTransactionRepository, MonthlyTransactionRepository>();

        return services;
    }
}
