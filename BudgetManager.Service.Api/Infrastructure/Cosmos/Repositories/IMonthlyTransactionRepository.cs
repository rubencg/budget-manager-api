using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface IMonthlyTransactionRepository
{
    Task<MonthlyTransaction?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    Task<List<MonthlyTransaction>> GetAllAsync(string userId, CancellationToken cancellationToken = default);

    Task<MonthlyTransaction> CreateAsync(MonthlyTransaction monthlyTransaction, CancellationToken cancellationToken = default);

    Task<MonthlyTransaction> UpdateAsync(MonthlyTransaction monthlyTransaction, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
