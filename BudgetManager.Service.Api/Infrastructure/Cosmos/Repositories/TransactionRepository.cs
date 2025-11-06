using System.Text;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly Container _container;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<TransactionRepository> logger)
    {
        var database = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = database.GetContainer(settings.Value.TransactionsContainer);
        _logger = logger;
    }

    public async Task<Transaction?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Transaction>(
                id,
                new PartitionKey(userId),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Transaction>> GetByMonthAsync(
        string userId,
        string yearMonth,
        TransactionType? transactionType = null,
        string? accountId = null,
        string? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.userId = @userId AND c.yearMonth = @yearMonth");

        var parameters = new List<(string, object)>
        {
            ("@userId", userId),
            ("@yearMonth", yearMonth)
        };

        if (transactionType.HasValue)
        {
            queryBuilder.Append(" AND c.transactionType = @transactionType");
            parameters.Add(("@transactionType", transactionType.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(accountId))
        {
            queryBuilder.Append(" AND c.accountId = @accountId");
            parameters.Add(("@accountId", accountId));
        }

        if (!string.IsNullOrEmpty(categoryId))
        {
            queryBuilder.Append(" AND c.categoryId = @categoryId");
            parameters.Add(("@categoryId", categoryId));
        }

        queryBuilder.Append(" ORDER BY c.date DESC");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString());
        foreach (var (name, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(name, value);
        }

        var iterator = _container.GetItemQueryIterator<Transaction>(
            queryDefinition,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        var results = new List<Transaction>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<Transaction>> GetRecentAsync(
        string userId,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition(
            $"SELECT TOP @count * FROM c WHERE c.userId = @userId ORDER BY c.date DESC, c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@count", count);

        var iterator = _container.GetItemQueryIterator<Transaction>(
            queryDefinition,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        var results = new List<Transaction>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<Transaction>> GetByAccountAsync(
        string userId,
        string accountId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.userId = @userId AND c.accountId = @accountId");

        var parameters = new List<(string, object)>
        {
            ("@userId", userId),
            ("@accountId", accountId)
        };

        if (startDate.HasValue)
        {
            queryBuilder.Append(" AND c.date >= @startDate");
            parameters.Add(("@startDate", startDate.Value));
        }

        if (endDate.HasValue)
        {
            queryBuilder.Append(" AND c.date <= @endDate");
            parameters.Add(("@endDate", endDate.Value));
        }

        queryBuilder.Append(" ORDER BY c.date DESC");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString());
        foreach (var (name, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(name, value);
        }

        var iterator = _container.GetItemQueryIterator<Transaction>(
            queryDefinition,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        var results = new List<Transaction>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<Transaction>> GetByCategoryAsync(
        string userId,
        string categoryId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT * FROM c WHERE c.userId = @userId AND c.categoryId = @categoryId");

        var parameters = new List<(string, object)>
        {
            ("@userId", userId),
            ("@categoryId", categoryId)
        };

        if (startDate.HasValue)
        {
            queryBuilder.Append(" AND c.date >= @startDate");
            parameters.Add(("@startDate", startDate.Value));
        }

        if (endDate.HasValue)
        {
            queryBuilder.Append(" AND c.date <= @endDate");
            parameters.Add(("@endDate", endDate.Value));
        }

        queryBuilder.Append(" ORDER BY c.date DESC");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString());
        foreach (var (name, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(name, value);
        }

        var iterator = _container.GetItemQueryIterator<Transaction>(
            queryDefinition,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

        var results = new List<Transaction>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(
            transaction,
            new PartitionKey(transaction.UserId),
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created transaction {TransactionId} - RU charge: {RequestCharge}",
            transaction.Id, response.RequestCharge);

        return response.Resource;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.UpdatedAt = DateTime.UtcNow;

        var response = await _container.ReplaceItemAsync(
            transaction,
            transaction.Id,
            new PartitionKey(transaction.UserId),
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Updated transaction {TransactionId} - RU charge: {RequestCharge}",
            transaction.Id, response.RequestCharge);

        return response.Resource;
    }

    public async Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<Transaction>(
            id,
            new PartitionKey(userId),
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted transaction {TransactionId}", id);
    }
}
