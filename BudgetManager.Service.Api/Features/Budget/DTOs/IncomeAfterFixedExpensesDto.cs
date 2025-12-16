using BudgetManager.Api.Domain.Enums;
using Newtonsoft.Json;

namespace BudgetManager.Service.Api.Features.Budget.DTOs;

public class IncomeAfterFixedExpensesDto
{
    [JsonProperty("incomesAfterMonthlyExpenses")]
    public IncomesAfterMonthlyExpensesData IncomesAfterMonthlyExpenses { get; set; } = new();
}

public class IncomesAfterMonthlyExpensesData
{
    [JsonProperty("total")]
    public decimal Total { get; set; }

    [JsonProperty("monthlyIncomes")]
    public BudgetSectionDto MonthlyIncomes { get; set; } = new();

    [JsonProperty("monthlyExpenses")]
    public BudgetSectionDto MonthlyExpenses { get; set; } = new();

    [JsonProperty("savings")]
    public BudgetSectionDto Savings { get; set; } = new();
}

public class BudgetSectionDto
{
    [JsonProperty("total")]
    public decimal Total { get; set; }

    [JsonProperty("items")]
    public List<BudgetSectionItemDto> Items { get; set; } = new();
}

public class BudgetSectionItemDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("userId")]
    public string UserId { get; set; } = null!;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("isApplied")]
    public bool IsApplied { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("dayOfMonth")]
    public int? DayOfMonth { get; set; }

    [JsonProperty("icon")]
    public string? Icon { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    // Monthly Transaction specific
    [JsonProperty("monthlyTransactionType")]
    public MonthlyTransactionType? MonthlyTransactionType { get; set; }

    [JsonProperty("accountId")]
    public string? AccountId { get; set; }

    [JsonProperty("accountName")]
    public string? AccountName { get; set; }

    [JsonProperty("categoryId")]
    public string? CategoryId { get; set; }

    [JsonProperty("categoryName")]
    public string? CategoryName { get; set; }

    [JsonProperty("subcategory")]
    public string? Subcategory { get; set; }
    
    // Saving specific
    [JsonProperty("goalAmount")]
    public decimal? GoalAmount { get; set; }

    [JsonProperty("savedAmount")]
    public decimal? SavedAmount { get; set; }
    
    [JsonProperty("amountPerMonth")]
    public decimal? AmountPerMonth { get; set; } // For saving, this is the default amount

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; } = null!;
}
