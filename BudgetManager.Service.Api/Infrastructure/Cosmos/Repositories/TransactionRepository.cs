using System.Text;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class TransactionRepository : CosmosRepositoryBase<Transaction>, ITransactionRepository
{
    protected override string EntityName => "Transaction";

    public TransactionRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<TransactionRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.TransactionsContainer),
            logger)
    {
    }

    public Task<Transaction?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public Task<List<Transaction>> GetByMonthAsync(
        string userId,
        string yearMonth,
        TransactionType? transactionType = null,
        string? accountId = null,
        string? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.yearMonth = @yearMonth");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString())
            .WithParameter("@yearMonth", yearMonth);

        if (transactionType.HasValue)
        {
            queryBuilder.Append(" AND c.transactionType = @transactionType");
            queryDefinition = queryDefinition.WithParameter("@transactionType", transactionType.Value.ToString());
        }

        if (!string.IsNullOrEmpty(accountId))
        {
            queryBuilder.Append(" AND c.accountId = @accountId");
            queryDefinition = queryDefinition.WithParameter("@accountId", accountId);
        }

        if (!string.IsNullOrEmpty(categoryId))
        {
            queryBuilder.Append(" AND c.categoryId = @categoryId");
            queryDefinition = queryDefinition.WithParameter("@categoryId", categoryId);
        }

        queryBuilder.Append(" ORDER BY c.date DESC");
        queryDefinition = new QueryDefinition(queryBuilder.ToString());

        // Re-apply parameters after rebuilding query
        queryDefinition = queryDefinition.WithParameter("@yearMonth", yearMonth);
        if (transactionType.HasValue)
            queryDefinition = queryDefinition.WithParameter("@transactionType", transactionType.Value.ToString());
        if (!string.IsNullOrEmpty(accountId))
            queryDefinition = queryDefinition.WithParameter("@accountId", accountId);
        if (!string.IsNullOrEmpty(categoryId))
            queryDefinition = queryDefinition.WithParameter("@categoryId", categoryId);

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetByMonthAsync), cancellationToken);
    }

    public Task<List<Transaction>> GetRecentAsync(
        string userId,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        var queryDefinition = new QueryDefinition(
            "SELECT TOP @count * FROM c ORDER BY c.date DESC, c.createdAt DESC")
            .WithParameter("@count", count);

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetRecentAsync), cancellationToken);
    }

    public Task<List<Transaction>> GetByAccountAsync(
        string userId,
        string accountId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));
        ValidateParameter(accountId, nameof(accountId));

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.accountId = @accountId");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString())
            .WithParameter("@accountId", accountId);

        if (startDate.HasValue)
        {
            queryBuilder.Append(" AND c.date >= @startDate");
            queryDefinition = queryDefinition.WithParameter("@startDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            queryBuilder.Append(" AND c.date <= @endDate");
            queryDefinition = queryDefinition.WithParameter("@endDate", endDate.Value);
        }

        queryBuilder.Append(" ORDER BY c.date DESC");
        queryDefinition = new QueryDefinition(queryBuilder.ToString())
            .WithParameter("@accountId", accountId);

        if (startDate.HasValue)
            queryDefinition = queryDefinition.WithParameter("@startDate", startDate.Value);
        if (endDate.HasValue)
            queryDefinition = queryDefinition.WithParameter("@endDate", endDate.Value);

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetByAccountAsync), cancellationToken);
    }

    public Task<List<Transaction>> GetByCategoryAsync(
        string userId,
        string categoryId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));
        ValidateParameter(categoryId, nameof(categoryId));

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.categoryId = @categoryId");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString())
            .WithParameter("@categoryId", categoryId);

        if (startDate.HasValue)
        {
            queryBuilder.Append(" AND c.date >= @startDate");
            queryDefinition = queryDefinition.WithParameter("@startDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            queryBuilder.Append(" AND c.date <= @endDate");
            queryDefinition = queryDefinition.WithParameter("@endDate", endDate.Value);
        }

        queryBuilder.Append(" ORDER BY c.date DESC");
        queryDefinition = new QueryDefinition(queryBuilder.ToString())
            .WithParameter("@categoryId", categoryId);

        if (startDate.HasValue)
            queryDefinition = queryDefinition.WithParameter("@startDate", startDate.Value);
        if (endDate.HasValue)
            queryDefinition = queryDefinition.WithParameter("@endDate", endDate.Value);

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetByCategoryAsync), cancellationToken);
    }

    public Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => CreateInternalAsync(transaction, cancellationToken);

    public Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => UpdateInternalAsync(transaction, cancellationToken);

    public Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
        => DeleteInternalAsync(id, userId, cancellationToken);
}
