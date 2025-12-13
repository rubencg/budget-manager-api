using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public record GetMonthlyTransactionsQuery : IRequest<List<MonthlyTransaction>>
{
}
