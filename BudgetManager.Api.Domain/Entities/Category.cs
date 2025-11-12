using BudgetManager.Api.Domain.Enums;
using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class Category : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("image")]
    public string Image { get; set; } = null!;

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("categoryType")]
    public CategoryType CategoryType { get; set; }

    [JsonProperty("subcategories")]
    public List<string>? Subcategories { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
