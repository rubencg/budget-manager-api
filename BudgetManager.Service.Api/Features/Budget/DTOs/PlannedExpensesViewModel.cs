using BudgetManager.Api.Domain.Entities;
using Newtonsoft.Json;

namespace BudgetManager.Service.Api.Features.Budget.DTOs;

public class PlannedExpensesResponseDto
{
    [JsonProperty("total")]
    public decimal Total { get; set; }

    [JsonProperty("plannedExpenses")]
    public List<PlannedExpenseViewDto> PlannedExpenses { get; set; } = new();

    [JsonProperty("items")]
    public List<BudgetSectionItemDto> Items { get; set; } = new();
}

public class PlannedExpenseViewDto : PlannedExpense
{
    [JsonProperty("amountSpent")]
    public decimal AmountSpent { get; set; }

    [JsonProperty("percentageSpent")]
    public decimal PercentageSpent { get; set; }

    [JsonProperty("amountLeft")]
    public decimal AmountLeft { get; set; }

    [JsonProperty("isCompleted")]
    public bool isCompleted => PercentageSpent == 100;
}
