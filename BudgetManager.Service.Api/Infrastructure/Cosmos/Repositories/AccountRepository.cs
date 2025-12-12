using BudgetManager.Api.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

public class AccountRepository : CosmosRepositoryBase<Account>, IAccountRepository
{
    protected override string EntityName => "Account";

    public AccountRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<AccountRepository> logger)
        : base(
            cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.AccountsContainer),
            logger)
    {
    }

    public Task<Account?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
        => GetByIdInternalAsync(id, userId, cancellationToken);

    public Task<List<Account>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // No WHERE clause needed - partition key already scopes the query to this userId
        var queryDefinition = new QueryDefinition("SELECT * FROM c");

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetByUserIdAsync), cancellationToken);
    }

    public Task<List<Account>> GetActiveAccountsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // Partition key scopes to userId, only need to filter by isArchived
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.isArchived = false");

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetActiveAccountsAsync), cancellationToken);
    }

    public Task<List<Account>> GetDashboardAccountsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // Filter by isArchived = false AND accountType != null
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.isArchived = false AND c.accountType != null");

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetDashboardAccountsAsync), cancellationToken);
    }

    public Task<List<Account>> GetArchivedAccountsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        // Filter by isArchived = true
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.isArchived = true");

        return ExecuteQueryAsync(queryDefinition, userId, nameof(GetArchivedAccountsAsync), cancellationToken);
    }


    public Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default)
        => CreateInternalAsync(account, cancellationToken);

    public Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default)
        => UpdateInternalAsync(account, cancellationToken);

    public Task DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
        => DeleteInternalAsync(id, userId, cancellationToken);
}
