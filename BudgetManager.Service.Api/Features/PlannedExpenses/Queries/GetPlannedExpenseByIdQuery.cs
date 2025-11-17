using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Queries;

public record GetPlannedExpenseByIdQuery : IRequest<PlannedExpense?>
{
    public required string PlannedExpenseId { get; init; }
}
