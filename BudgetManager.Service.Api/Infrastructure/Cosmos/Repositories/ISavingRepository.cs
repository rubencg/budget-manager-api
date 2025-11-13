using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface ISavingRepository
{
    Task<Saving?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<List<Saving>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Saving> CreateAsync(Saving saving, CancellationToken cancellationToken = default);
    Task<Saving> UpdateAsync(Saving saving, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
