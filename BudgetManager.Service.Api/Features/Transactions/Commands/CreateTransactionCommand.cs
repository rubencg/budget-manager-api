using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public record CreateTransactionCommand : IRequest<Transaction>
{
    public required string UserId { get; init; }
    public required TransactionType TransactionType { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime Date { get; init; }

    // Account references
    public required string AccountId { get; init; }
    public required string AccountName { get; init; }
    public string? ToAccountId { get; init; }
    public string? ToAccountName { get; init; }

    // Category references
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryImage { get; init; }
    public string? CategoryColor { get; init; }
    public string? Subcategory { get; init; }

    // Transaction details
    public string? Notes { get; init; }
    public bool IsApplied { get; init; } = true;
    public decimal? AppliedAmount { get; init; }

    // Monthly/recurring
    public bool IsMonthly { get; init; }
    public bool IsRecurring { get; init; }
    public string? MonthlyKey { get; init; }
    public int? RecurringTimes { get; init; }
    public string? RecurringType { get; init; }

    // References
    public string? SavingKey { get; init; }
    public string? TransferId { get; init; }
    public bool RemoveFromSpendingPlan { get; init; }

    public Dictionary<string, object>? Metadata { get; init; }
}
