using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Api.Features.Budget.DTOs;
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
    CalendarView CalendarView,
    List<BudgetSectionItemDto> Savings
);

public record DashboardBalance(
    decimal Total,
    int AccountCount
);

public record CalendarView(
    string YearMonth,
    int TransfersCount,
    int ExpensesCount,
    int IncomesCount
);

