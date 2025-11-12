using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByCategoryTypeAsync(string userId, CategoryType categoryType, CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
