using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public record GetTransactionsByMonthQuery : IRequest<GetTransactionsByMonthQueryResult>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public TransactionType? TransactionType { get; init; }
    public string? AccountId { get; init; }
    public string? CategoryId { get; init; }
}

public record GetTransactionsByMonthQueryResult(
    string YearMonth,
    int TotalCount,
    decimal TotalExpenses,
    decimal TotalIncome,
    List<Transaction> Transactions
);