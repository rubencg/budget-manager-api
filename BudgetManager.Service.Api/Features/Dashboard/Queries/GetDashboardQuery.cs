using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Dashboard.Queries;

public record GetDashboardQuery : IRequest<GetDashboardQueryResult>
{
    public int? Year { get; init; }
    public int? Month { get; init; }
}

public record GetDashboardQueryResult(
    DashboardBalance Balance,
    List<Transaction> RecentTransactions,
    CalendarView CalendarView
);

public record DashboardBalance(
    decimal Total,
    int AccountCount
);

public record CalendarView(
    string YearMonth,
    List<DayActivitySummary> Days
);

public record DayActivitySummary(
    DateTime Date,
    bool HasExpenses,
    bool HasNotAppliedExpenses,
    bool HasIncome,
    bool HasNotAppliedIncome,
    bool HasTransfers,
    int ExpenseCount,
    int NotAppliedExpenseCount,
    int IncomeCount,
    int NotAppliedIncomeCount,
    int TransferCount
);
