using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class SavingRepository : CosmosRepositoryBase<Saving>, ISavingRepository
{
    protected override string EntityName => "Saving";

    public SavingRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<SavingRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.PlannedItemsContainer),
            logger)
    {
    }

    public Task<Saving?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public async Task<List<Saving>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // Filter by itemType to only get saving items (not plannedExpense)
        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.itemType = @itemType")
            .WithParameter("@itemType", DomainConstants.SavingsType);

        return await ExecuteQueryAsync(
            queryDefinition,
            userId,
            nameof(GetByUserIdAsync),
            cancellationToken);
    }

    public Task<Saving> CreateAsync(
        Saving saving,
        CancellationToken cancellationToken = default) => CreateInternalAsync(saving, cancellationToken);

    public Task<Saving> UpdateAsync(
        Saving saving,
        CancellationToken cancellationToken = default) => UpdateInternalAsync(saving, cancellationToken);

    public Task DeleteAsync(
        string id,
        string userId,
        CancellationToken cancellationToken = default) => DeleteInternalAsync(id, userId, cancellationToken);
}
