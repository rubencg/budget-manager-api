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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetDashboardHandler> _logger;

    public GetDashboardHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ISavingRepository savingRepository,
        ICurrentUserService currentUserService,
        ILogger<GetDashboardHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _savingRepository = savingRepository;
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

    private async Task<DashboardBalance> GetBalanceAsync(string userId, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetActiveAccountsAsync(userId, cancellationToken);

        var totalBalance = accounts.Sum(a => a.CurrentBalance);
        var accountCount = accounts.Count;

        return new DashboardBalance(totalBalance, accountCount);
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
