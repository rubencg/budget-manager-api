using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Api.Features.Budget.DTOs;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Dashboard.Queries;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, GetDashboardQueryResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISavingRepository _savingRepository;
    private readonly IMonthlyTransactionRepository _monthlyTransactionRepository;
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetDashboardHandler> _logger;

    public GetDashboardHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ISavingRepository savingRepository,
        IMonthlyTransactionRepository monthlyTransactionRepository,
        IPlannedExpenseRepository plannedExpenseRepository,
        ICurrentUserService currentUserService,
        ILogger<GetDashboardHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _savingRepository = savingRepository;
        _monthlyTransactionRepository = monthlyTransactionRepository;
        _plannedExpenseRepository = plannedExpenseRepository;
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

        // 1. Get balance: sum of all active account balances adjusted by pending items
        var balance = await GetBalanceAsync(userId, targetDate, cancellationToken);

        // 2. Get last 5 transactions
        var recentTransactions = await _transactionRepository.GetRecentAsync(
            userId,
            year,
            month,
            5,
            cancellationToken);

        // 3. Get monthly stats: counts and transactions for savings
        var (calendarView, monthTransactions) = await GetMonthlyStatsAsync(
            userId,
            yearMonth,
            cancellationToken);

        // 4. Get savings
        var savings = await GetSavingsAsync(userId, monthTransactions, cancellationToken);

        return new GetDashboardQueryResult(
            balance,
            recentTransactions,
            calendarView,
            savings);
    }

    private async Task<DashboardBalance> GetBalanceAsync(string userId, DateTime targetDate, CancellationToken cancellationToken)
    {
        var year = targetDate.Year;
        var month = targetDate.Month;
        var yearMonth = $"{year}-{month:D2}";
        var today = DateTime.UtcNow;

        var allActiveAccounts = await _accountRepository.GetActiveAccountsAsync(userId, cancellationToken);
        var budgetAccounts = allActiveAccounts.Where(a => a.SumsToMonthlyBudget).ToList();
        
        var accountCount = budgetAccounts.Count;
        var currentTotalBalance = budgetAccounts.Sum(a => a.CurrentBalance);

        // Fetch data for adjustments
        var monthlyTransactionsTask = _monthlyTransactionRepository.GetAllAsync(userId, cancellationToken);
        var savingsTask = _savingRepository.GetByUserIdAsync(userId, cancellationToken);
        var transactionsTask = _transactionRepository.GetByMonthAsync(userId, yearMonth, cancellationToken: cancellationToken);
        var plannedExpensesTask = _plannedExpenseRepository.GetByUserIdAsync(userId, cancellationToken);

        await Task.WhenAll(monthlyTransactionsTask, savingsTask, transactionsTask, plannedExpensesTask);

        var monthlyTransactions = monthlyTransactionsTask.Result;
        var savings = savingsTask.Result;
        var transactions = transactionsTask.Result;
        var allPlannedExpenses = plannedExpensesTask.Result;

        bool isPastMonth = today.Year > year || (today.Year == year && today.Month > month);

        if (isPastMonth)
        {
            // For past months, follow the logic: incomesAmount - expensesAmount
            var monthIncomes = transactions
                .Where(t => t.TransactionType == TransactionType.Income)
                .Sum(t => t.Amount);
            var monthExpenses = transactions
                .Where(t => t.TransactionType == TransactionType.Expense)
                .Sum(t => t.Amount);
            
            return new DashboardBalance(monthIncomes - monthExpenses, accountCount);
        }

        // For current/future months:
        // currentBalance + unpaidMonthlyIncomes + unpaidIncomes - savings - plannedExpenses

        // 1. Unpaid Monthly Items (Recurring items from MonthlyTransaction)
        var unpaidNetMonthlyAmount = monthlyTransactions
            .Where(mt => !transactions.Any(t => t.MonthlyKey == mt.Id))
            .Sum(mt => mt.MonthlyTransactionType == MonthlyTransactionType.Income ? mt.Amount : -mt.Amount);

        // 2. Unpaid Transactions (Transactions already created but not yet applied/affecting balance)
        var unpaidNetTransactionAmount = transactions
            .Where(t => !t.IsApplied)
            .Sum(t => 
            {
                if (t.TransactionType == TransactionType.Income) return t.Amount;
                if (t.TransactionType == TransactionType.Expense) return -t.Amount;
                return 0;
            });

        // 3. Unpaid Savings
        var unpaidSavingsAmount = savings
            .Where(s => !transactions.Any(t => t.SavingKey == s.Id))
            .Sum(s => s.AmountPerMonth);

        // 4. Planned Expenses (Amount left to spend)
        var activePlannedExpenses = allPlannedExpenses.Where(pe =>
            pe.IsRecurring ||
            (pe.Date.HasValue && pe.Date.Value.Year == year && pe.Date.Value.Month == month)
        ).ToList();

        var unpaidPlannedExpensesAmount = activePlannedExpenses
            .Select(pe =>
            {
                // Match transactions by category and subcategory
                var spent = transactions
                    .Where(t => t.TransactionType == TransactionType.Expense &&
                                t.CategoryId == pe.CategoryId &&
                                (string.IsNullOrEmpty(pe.SubCategory) || t.Subcategory == pe.SubCategory))
                    .Sum(t => t.Amount);
                return Math.Max(0, pe.TotalAmount - spent);
            })
            .Sum();

        var finalBalance = currentTotalBalance + 
                           unpaidNetMonthlyAmount + 
                           unpaidNetTransactionAmount - 
                           unpaidSavingsAmount - 
                           unpaidPlannedExpensesAmount;

        return new DashboardBalance(finalBalance, accountCount);
    }

    private async Task<(CalendarView, List<BudgetManager.Api.Domain.Entities.Transaction>)> GetMonthlyStatsAsync(
        string userId,
        string yearMonth,
        CancellationToken cancellationToken)
    {
        // Get all transactions for the month
        var transactions = await _transactionRepository.GetByMonthAsync(
            userId,
            yearMonth,
            cancellationToken: cancellationToken);

        var transfersCount = transactions.Count(t => t.TransactionType == TransactionType.Transfer);
        var expensesCount = transactions.Count(t => t.TransactionType == TransactionType.Expense);
        var incomesCount = transactions.Count(t => t.TransactionType == TransactionType.Income);

        return (new CalendarView(yearMonth, transfersCount, expensesCount, incomesCount), transactions);
    }

    private async Task<List<BudgetSectionItemDto>> GetSavingsAsync(
        string userId,
        List<BudgetManager.Api.Domain.Entities.Transaction> transactions,
        CancellationToken cancellationToken)
    {
        var savings = await _savingRepository.GetByUserIdAsync(userId, cancellationToken);

        return savings.Select(s =>
        {
            var linkedTransaction = transactions.FirstOrDefault(t => t.SavingKey == s.Id);
            var isApplied = linkedTransaction != null;
            var amount = isApplied ? linkedTransaction!.Amount : s.AmountPerMonth;

            return new BudgetSectionItemDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Amount = amount,
                IsApplied = isApplied,
                TransactionId = linkedTransaction?.Id,
                Name = s.Name,
                Icon = s.Icon,
                Color = s.Color,
                GoalAmount = s.GoalAmount,
                SavedAmount = s.SavedAmount,
                AmountPerMonth = s.AmountPerMonth,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                Type = "saving"
            };
        }).ToList();
    }
}
