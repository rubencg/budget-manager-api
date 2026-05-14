using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public record GetOverdueMonthlyTransactionsQuery : IRequest<List<MonthlyTransaction>>
{
    public string UserId { get; init; } = null!;
    public DateOnly Date { get; init; }
}
