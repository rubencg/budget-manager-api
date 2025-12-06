using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    Task<List<Transaction>> GetByMonthAsync(
        string userId,
        string yearMonth,
        TransactionType? transactionType = null,
        string? accountId = null,
        string? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<List<Transaction>> GetRecentAsync(
        string userId,
        int year,
        int month,
        int count = 20,
        CancellationToken cancellationToken = default);

    Task<List<Transaction>> GetByAccountAsync(
        string userId,
        string accountId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<List<Transaction>> GetByCategoryAsync(
        string userId,
        string categoryId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
