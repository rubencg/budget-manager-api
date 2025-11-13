using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class PlannedExpense : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("itemType")]
    public string ItemType { get; set; } = DomainConstants.PlannedExpensesType;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("isRecurring")]
    public bool IsRecurring { get; set; }

    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    // Category information
    [JsonProperty("categoryId")]
    public string? CategoryId { get; set; }

    [JsonProperty("categoryName")]
    public string? CategoryName { get; set; }

    [JsonProperty("categoryImage")]
    public string? CategoryImage { get; set; }

    [JsonProperty("categoryColor")]
    public string? CategoryColor { get; set; }

    [JsonProperty("subCategory")]
    public string? SubCategory { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
