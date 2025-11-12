using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class AccountType
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;
}