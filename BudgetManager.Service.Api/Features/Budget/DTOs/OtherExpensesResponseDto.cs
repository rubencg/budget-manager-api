namespace BudgetManager.Service.Api.Features.Budget.DTOs;

public class OtherExpensesResponseDto
{
    public List<BudgetSectionItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}
