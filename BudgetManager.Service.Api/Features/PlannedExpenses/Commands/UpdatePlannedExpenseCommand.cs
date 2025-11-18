using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public record UpdatePlannedExpenseCommand : CreatePlannedExpenseCommand, IRequest<PlannedExpense>
{
    public string? PlannedExpenseId { get; init; }
}
