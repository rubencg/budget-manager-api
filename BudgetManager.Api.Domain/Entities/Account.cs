using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class Account : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("currentBalance")]
    public decimal CurrentBalance { get; set; }

    [JsonProperty("accountType")]
    public AccountType AccountType { get; set; } = null!;

    [JsonProperty("isArchived")]
    public bool IsArchived { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("image")]
    public string? Image { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [JsonProperty("_type")]
    public string Type { get; set; } = "account";

    [JsonProperty("sumsToMonthlyBudget")]
    public bool SumsToMonthlyBudget { get; set; }

    [JsonProperty("availableCredit")]
    public decimal? AvailableCredit { get; set; }

    [JsonProperty("remainingCredit")]
    public decimal? RemainingCredit => AvailableCredit.HasValue ? (AvailableCredit.Value + CurrentBalance) : null;
}
