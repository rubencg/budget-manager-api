using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public record GetMonthlyTransactionByIdQuery : IRequest<MonthlyTransaction?>
{
    public string Id { get; init; } = null!;
}
