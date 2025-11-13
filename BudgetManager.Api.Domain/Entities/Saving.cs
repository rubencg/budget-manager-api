using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class Saving : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("itemType")]
    public string ItemType { get; set; } = DomainConstants.SavingsType;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("icon")]
    public string? Icon { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("goalAmount")]
    public decimal GoalAmount { get; set; }

    [JsonProperty("savedAmount")]
    public decimal SavedAmount { get; set; }

    [JsonProperty("amountPerMonth")]
    public decimal AmountPerMonth { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
