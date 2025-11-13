using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public record DeletePlannedExpenseCommand : IRequest
{
    public required string PlannedExpenseId { get; init; }
    public required string UserId { get; init; }
}
