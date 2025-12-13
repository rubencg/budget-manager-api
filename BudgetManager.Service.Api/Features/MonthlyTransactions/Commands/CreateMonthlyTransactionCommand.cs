using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public record CreateMonthlyTransactionCommand : IRequest<MonthlyTransaction>
{
    public decimal Amount { get; init; }
    public MonthlyTransactionType MonthlyTransactionType { get; init; }
    public string? Notes { get; init; }
    public int DayOfMonth { get; init; }
    public string AccountId { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? Subcategory { get; init; }
}
