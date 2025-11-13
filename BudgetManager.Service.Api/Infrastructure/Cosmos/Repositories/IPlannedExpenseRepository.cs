using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface IPlannedExpenseRepository
{
    Task<PlannedExpense?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<List<PlannedExpense>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<PlannedExpense> CreateAsync(PlannedExpense plannedExpense, CancellationToken cancellationToken = default);
    Task<PlannedExpense> UpdateAsync(PlannedExpense plannedExpense, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
