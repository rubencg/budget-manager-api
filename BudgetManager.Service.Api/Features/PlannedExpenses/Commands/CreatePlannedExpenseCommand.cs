using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public record CreatePlannedExpenseCommand : IRequest<PlannedExpense>
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required DateTime Date { get; init; }
    public bool IsRecurring { get; init; }
    public decimal TotalAmount { get; init; }

    // Category information
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryImage { get; init; }
    public string? CategoryColor { get; init; }
    public string? SubCategory { get; init; }
}
