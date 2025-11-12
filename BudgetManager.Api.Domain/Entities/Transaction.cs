using BudgetManager.Api.Domain.Enums;
using Newtonsoft.Json;

namespace BudgetManager.Api.Domain.Entities;

public class Transaction : ICosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("transactionType")]
    public TransactionType TransactionType { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    // Account references
    [JsonProperty("accountId")]
    public string AccountId { get; set; } = null!;

    [JsonProperty("accountName")]
    public string AccountName { get; set; } = null!;

    [JsonProperty("toAccountId")]
    public string? ToAccountId { get; set; }

    [JsonProperty("toAccountName")]
    public string? ToAccountName { get; set; }

    // Category references
    [JsonProperty("categoryId")]
    public string? CategoryId { get; set; }

    [JsonProperty("categoryName")]
    public string? CategoryName { get; set; }

    [JsonProperty("categoryImage")]
    public string? CategoryImage { get; set; }

    [JsonProperty("categoryColor")]
    public string? CategoryColor { get; set; }

    [JsonProperty("subcategory")]
    public string? Subcategory { get; set; }

    // Transaction details
    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("isApplied")]
    public bool IsApplied { get; set; }

    [JsonProperty("appliedAmount")]
    public decimal? AppliedAmount { get; set; }

    // Monthly/recurring
    [JsonProperty("isMonthly")]
    public bool IsMonthly { get; set; }

    [JsonProperty("isRecurring")]
    public bool IsRecurring { get; set; }

    [JsonProperty("monthlyKey")]
    public string? MonthlyKey { get; set; }

    [JsonProperty("recurringTimes")]
    public int? RecurringTimes { get; set; }

    [JsonProperty("recurringType")]
    public string? RecurringType { get; set; }

    // References
    [JsonProperty("savingKey")]
    public string? SavingKey { get; set; }

    [JsonProperty("transferId")]
    public string? TransferId { get; set; }

    [JsonProperty("removeFromSpendingPlan")]
    public bool RemoveFromSpendingPlan { get; set; }

    // Indexing fields
    [JsonProperty("yearMonth")]
    public string YearMonth { get; set; } = null!;

    [JsonProperty("year")]
    public int Year { get; set; }

    [JsonProperty("month")]
    public int Month { get; set; }

    [JsonProperty("day")]
    public int Day { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [JsonProperty("_type")]
    public string Type { get; set; } = "transaction";
}