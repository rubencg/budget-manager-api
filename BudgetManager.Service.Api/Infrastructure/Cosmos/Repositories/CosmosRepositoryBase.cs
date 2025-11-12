using BudgetManager.Api.Domain.Entities;
using Microsoft.Azure.Cosmos;

namespace BudgetManager.Service.Infrastructure.Cosmos.Repositories;

/// <summary>
/// Base repository class for Cosmos DB operations with common CRUD functionality
/// </summary>
/// <typeparam name="TEntity">Entity type that implements ICosmosEntity</typeparam>
public abstract class CosmosRepositoryBase<TEntity> where TEntity : ICosmosEntity
{
    protected readonly Container Container;
    protected readonly ILogger Logger;
    protected abstract string EntityName { get; }

    protected CosmosRepositoryBase(Container container, ILogger logger)
    {
        Container = container;
        Logger = logger;
    }

    /// <summary>
    /// Get entity by ID with proper error handling and logging
    /// </summary>
    protected async Task<TEntity?> GetByIdInternalAsync(
        string id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateParameters(id, nameof(id), userId, nameof(userId));

        try
        {
            var response = await Container.ReadItemAsync<TEntity>(
                id,
                new PartitionKey(userId),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogDebug("{EntityName} {EntityId} not found for user {UserId}", EntityName, id, userId);
            return default;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex,
                "Cosmos DB error getting {EntityName} {EntityId} for user {UserId}. Status: {StatusCode}",
                EntityName, id, userId, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Unexpected error getting {EntityName} {EntityId} for user {UserId}",
                EntityName, id, userId);
            throw;
        }
    }

    /// <summary>
    /// Execute a query and return all results with proper error handling
    /// </summary>
    protected async Task<List<TEntity>> ExecuteQueryAsync(
        QueryDefinition queryDefinition,
        string userId,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ValidateParameter(userId, nameof(userId));

        try
        {
            var iterator = Container.GetItemQueryIterator<TEntity>(
                queryDefinition,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

            var results = new List<TEntity>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);

                Logger.LogDebug(
                    "{OperationName} page - RU charge: {RequestCharge}",
                    operationName, response.RequestCharge);
            }

            return results;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex,
                "Cosmos DB error in {OperationName} for user {UserId}. Status: {StatusCode}",
                operationName, userId, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Unexpected error in {OperationName} for user {UserId}",
                operationName, userId);
            throw;
        }
    }

    /// <summary>
    /// Create entity with timestamp management
    /// </summary>
    protected async Task<TEntity> CreateInternalAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAt = now;
        entity.UpdatedAt = now;

        var response = await Container.CreateItemAsync(
            entity,
            new PartitionKey(entity.UserId),
            cancellationToken: cancellationToken);

        Logger.LogInformation(
            "Created {EntityName} {EntityId} - RU charge: {RequestCharge}",
            EntityName, entity.Id, response.RequestCharge);

        // Return the entity we just created (response.Resource is null due to EnableContentResponseOnWrite = false)
        return entity;
    }

    /// <summary>
    /// Update entity with timestamp management
    /// </summary>
    protected async Task<TEntity> UpdateInternalAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var response = await Container.ReplaceItemAsync(
            entity,
            entity.Id,
            new PartitionKey(entity.UserId),
            cancellationToken: cancellationToken);

        Logger.LogInformation(
            "Updated {EntityName} {EntityId} - RU charge: {RequestCharge}",
            EntityName, entity.Id, response.RequestCharge);

        // Return the entity we just updated (response.Resource is null due to EnableContentResponseOnWrite = false)
        return entity;
    }

    /// <summary>
    /// Delete entity with proper error handling
    /// </summary>
    protected async Task DeleteInternalAsync(
        string id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateParameters(id, nameof(id), userId, nameof(userId));

        try
        {
            var response = await Container.DeleteItemAsync<TEntity>(
                id,
                new PartitionKey(userId),
                cancellationToken: cancellationToken);

            Logger.LogInformation(
                "Deleted {EntityName} {EntityId} for user {UserId} - RU charge: {RequestCharge}",
                EntityName, id, userId, response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogWarning(
                "Attempted to delete non-existent {EntityName} {EntityId} for user {UserId}",
                EntityName, id, userId);
            throw;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex,
                "Cosmos DB error deleting {EntityName} {EntityId} for user {UserId}. Status: {StatusCode}",
                EntityName, id, userId, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Unexpected error deleting {EntityName} {EntityId} for user {UserId}",
                EntityName, id, userId);
            throw;
        }
    }

    /// <summary>
    /// Validate a single parameter
    /// </summary>
    protected void ValidateParameter(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }

    /// <summary>
    /// Validate multiple parameters
    /// </summary>
    protected void ValidateParameters(string value1, string name1, string value2, string name2)
    {
        ValidateParameter(value1, name1);
        ValidateParameter(value2, name2);
    }
}
