using BudgetManager.Api.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class PlannedExpenseRepository : CosmosRepositoryBase<PlannedExpense>, IPlannedExpenseRepository
{
    protected override string EntityName => "PlannedExpense";

    public PlannedExpenseRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<PlannedExpenseRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.PlannedItemsContainer),
            logger)
    {
    }

    public Task<PlannedExpense?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public async Task<List<PlannedExpense>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // Filter by itemType to only get plannedExpense items (not savings)
        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.itemType = @itemType")
            .WithParameter("@itemType", "plannedExpense");

        return await ExecuteQueryAsync(
            queryDefinition,
            userId,
            nameof(GetByUserIdAsync),
            cancellationToken);
    }

    public Task<PlannedExpense> CreateAsync(
        PlannedExpense plannedExpense,
        CancellationToken cancellationToken = default) => CreateInternalAsync(plannedExpense, cancellationToken);

    public Task<PlannedExpense> UpdateAsync(
        PlannedExpense plannedExpense,
        CancellationToken cancellationToken = default) => UpdateInternalAsync(plannedExpense, cancellationToken);

    public Task DeleteAsync(
        string id,
        string userId,
        CancellationToken cancellationToken = default) => DeleteInternalAsync(id, userId, cancellationToken);
}
