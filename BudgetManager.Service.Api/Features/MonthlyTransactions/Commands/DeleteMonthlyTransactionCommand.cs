using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public record DeleteMonthlyTransactionCommand : IRequest
{
    public string Id { get; init; } = null!;
}
