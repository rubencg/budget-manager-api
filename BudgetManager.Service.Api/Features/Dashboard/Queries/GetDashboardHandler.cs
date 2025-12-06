using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Dashboard.Queries;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, GetDashboardQueryResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetDashboardHandler> _logger;

    public GetDashboardHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<GetDashboardHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetDashboardQueryResult> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Determine the target year/month (default to current if not specified)
        var targetDate = request.Year.HasValue && request.Month.HasValue
            ? new DateTime(request.Year.Value, request.Month.Value, 1)
            : DateTime.UtcNow;

        var year = targetDate.Year;
        var month = targetDate.Month;
        var yearMonth = $"{year}-{month:D2}";

        _logger.LogInformation(
            "Getting dashboard for user {UserId} for {YearMonth}",
            userId, yearMonth);

        // 1. Get balance: sum of all active account balances
        var balance = await GetBalanceAsync(userId, cancellationToken);

        // 2. Get last 5 transactions
        var recentTransactions = await _transactionRepository.GetRecentAsync(
            userId,
            year,
            month,
            5,
            cancellationToken);

        // 3. Get calendar view: daily activity for the month
        var calendarView = await GetCalendarViewAsync(
            userId,
            yearMonth,
            year,
            month,
            cancellationToken);

        return new GetDashboardQueryResult(
            balance,
            recentTransactions,
            calendarView);
    }

    private async Task<DashboardBalance> GetBalanceAsync(string userId, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetActiveAccountsAsync(userId, cancellationToken);

        var totalBalance = accounts.Sum(a => a.CurrentBalance);
        var accountCount = accounts.Count;

        return new DashboardBalance(totalBalance, accountCount);
    }

    private async Task<CalendarView> GetCalendarViewAsync(
        string userId,
        string yearMonth,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        // Get all transactions for the month
        var transactions = await _transactionRepository.GetByMonthAsync(
            userId,
            yearMonth,
            cancellationToken: cancellationToken);

        // Get number of days in the month
        var daysInMonth = DateTime.DaysInMonth(year, month);

        // Group transactions by day
        var transactionsByDay = transactions
            .GroupBy(t => t.Day)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Build day summaries for each day of the month
        var daySummaries = new List<DayActivitySummary>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var dayTransactions = transactionsByDay.GetValueOrDefault(day) ?? new List<BudgetManager.Api.Domain.Entities.Transaction>();

            var expenses = dayTransactions.Where(t =>
                t.TransactionType == TransactionType.Expense && t.IsApplied).ToList();

            var notAppliedExpenses = dayTransactions.Where(t =>
                t.TransactionType == TransactionType.Expense && !t.IsApplied).ToList();

            var incomes = dayTransactions.Where(t =>
                t.TransactionType == TransactionType.Income && t.IsApplied).ToList();

            var notAppliedIncomes = dayTransactions.Where(t =>
                t.TransactionType == TransactionType.Income && !t.IsApplied).ToList();

            var transfers = dayTransactions.Where(t =>
                t.TransactionType == TransactionType.Transfer).ToList();

            var dayActivity = new DayActivitySummary(
                Date: new DateTime(year, month, day),
                HasExpenses: expenses.Any(),
                HasNotAppliedExpenses: notAppliedExpenses.Any(),
                HasIncome: incomes.Any(),
                HasNotAppliedIncome: notAppliedIncomes.Any(),
                HasTransfers: transfers.Any(),
                ExpenseCount: expenses.Count,
                NotAppliedExpenseCount: notAppliedExpenses.Count,
                IncomeCount: incomes.Count,
                NotAppliedIncomeCount: notAppliedIncomes.Count,
                TransferCount: transfers.Count
            );

            daySummaries.Add(dayActivity);
        }

        return new CalendarView(yearMonth, daySummaries);
    }
}
