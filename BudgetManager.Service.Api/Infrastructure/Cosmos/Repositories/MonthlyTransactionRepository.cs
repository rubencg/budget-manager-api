using BudgetManager.Api.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class MonthlyTransactionRepository : CosmosRepositoryBase<MonthlyTransaction>, IMonthlyTransactionRepository
{
    protected override string EntityName => "MonthlyTransaction";

    public MonthlyTransactionRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<MonthlyTransactionRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.MonthlyTransactionsContainer),
            logger)
    {
    }

    public Task<MonthlyTransaction?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public async Task<List<MonthlyTransaction>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c ORDER BY c.dayOfMonth ASC");
        return await ExecuteQueryAsync(queryDefinition, userId, nameof(GetAllAsync), cancellationToken);
    }

    public Task<MonthlyTransaction> CreateAsync(MonthlyTransaction monthlyTransaction, CancellationToken cancellationToken = default)
        => CreateInternalAsync(monthlyTransaction, cancellationToken);

    public Task<MonthlyTransaction> UpdateAsync(MonthlyTransaction monthlyTransaction, CancellationToken cancellationToken = default)
        => UpdateInternalAsync(monthlyTransaction, cancellationToken);

    public Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
        => DeleteInternalAsync(id, userId, cancellationToken);
}
