using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    Task<List<Account>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<List<Account>> GetActiveAccountsAsync(string userId, CancellationToken cancellationToken = default);

    Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default);

    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
