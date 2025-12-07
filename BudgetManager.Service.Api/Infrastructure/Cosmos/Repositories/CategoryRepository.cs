using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class CategoryRepository : CosmosRepositoryBase<Category>, ICategoryRepository
{
    protected override string EntityName => "Category";

    public CategoryRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CategoryRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.CategoriesContainer),
            logger)
    {
    }

    public Task<Category?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public async Task<List<Category>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        var queryDefinition = new QueryDefinition("SELECT * FROM c");

        return await ExecuteQueryAsync(queryDefinition, userId, nameof(GetByUserIdAsync), cancellationToken);
    }

    public async Task<List<Category>> GetByCategoryTypeAsync(
        string userId,
        CategoryType categoryType,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.categoryType = @categoryType ORDER BY c.name")
            .WithParameter("@categoryType", categoryType.ToString().ToLowerInvariant());

        return await ExecuteQueryAsync(
            queryDefinition,
            userId,
            nameof(GetByCategoryTypeAsync),
            cancellationToken);
    }

    public Task<Category> CreateAsync(
        Category category,
        CancellationToken cancellationToken = default) => CreateInternalAsync(category, cancellationToken);

    public Task<Category> UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default) => UpdateInternalAsync(category, cancellationToken);

    public Task DeleteAsync(
        string id,
        string userId,
        CancellationToken cancellationToken = default) => DeleteInternalAsync(id, userId, cancellationToken);
}
