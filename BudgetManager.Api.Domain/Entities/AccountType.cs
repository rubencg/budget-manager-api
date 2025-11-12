using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class AccountType
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("category")]
    public string Category { get; set; } = null!;
}