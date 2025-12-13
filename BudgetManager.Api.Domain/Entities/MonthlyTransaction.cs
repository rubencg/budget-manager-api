using BudgetManager.Api.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BudgetManager.Api.Domain.Entities;

public class MonthlyTransaction : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("monthlyTransactionType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public MonthlyTransactionType MonthlyTransactionType { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("dayOfMonth")]
    public int DayOfMonth { get; set; }

    [JsonProperty("accountId")]
    public string AccountId { get; set; } = null!;

    [JsonProperty("accountName")]
    public string AccountName { get; set; } = null!;

    [JsonProperty("categoryId")]
    public string? CategoryId { get; set; }

    [JsonProperty("categoryName")]
    public string? CategoryName { get; set; }

    [JsonProperty("subcategory")]
    public string? Subcategory { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("_type")]
    public string Type { get; set; } = "monthlyTransaction";
}
