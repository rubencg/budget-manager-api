namespace BudgetManager.Api.Domain.Entities;

/// <summary>
/// Base interface for Cosmos DB entities with common properties
/// </summary>
public interface ICosmosEntity
{
    string Id { get; set; }
    string UserId { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
