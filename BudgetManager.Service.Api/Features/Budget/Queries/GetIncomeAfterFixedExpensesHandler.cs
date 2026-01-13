using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Api.Features.Budget.DTOs;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Api.Features.Budget.Queries;

public class GetIncomeAfterFixedExpensesHandler : IRequestHandler<GetIncomeAfterFixedExpensesQuery, IncomeAfterFixedExpensesDto>
{
    private readonly IMonthlyTransactionRepository _monthlyTransactionRepository;
    private readonly ISavingRepository _savingRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<GetIncomeAfterFixedExpensesHandler> _logger;

    public GetIncomeAfterFixedExpensesHandler(
        IMonthlyTransactionRepository monthlyTransactionRepository,
        ISavingRepository savingRepository,
        ITransactionRepository transactionRepository,
        ILogger<GetIncomeAfterFixedExpensesHandler> logger)
    {
        _monthlyTransactionRepository = monthlyTransactionRepository;
        _savingRepository = savingRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<IncomeAfterFixedExpensesDto> Handle(GetIncomeAfterFixedExpensesQuery request, CancellationToken cancellationToken)
    {
        var yearMonth = $"{request.Year}-{request.Month:D2}";
        var userId = request.UserId;

        // 1. Fetch data
        var monthlyTransactionsTask = _monthlyTransactionRepository.GetAllAsync(userId, cancellationToken);
        var savingsTask = _savingRepository.GetByUserIdAsync(userId, cancellationToken);
        var transactionsTask = _transactionRepository.GetByMonthAsync(userId, yearMonth, cancellationToken: cancellationToken);

        await Task.WhenAll(monthlyTransactionsTask, savingsTask, transactionsTask);

        var monthlyTransactions = monthlyTransactionsTask.Result;
        var savings = savingsTask.Result;
        var transactions = transactionsTask.Result;

        var dto = new IncomeAfterFixedExpensesDto();

        // 2. Process Monthly Incomes
        var monthlyIncomes = monthlyTransactions
            .Where(mt => mt.MonthlyTransactionType == MonthlyTransactionType.Income)
            .Select(mt => {
                 var linkedTransaction = transactions.FirstOrDefault(t => t.MonthlyKey == mt.Id);
                 var isApplied = linkedTransaction != null;
                 var amount = isApplied ? linkedTransaction!.Amount : mt.Amount;
                 
                 return new BudgetSectionItemDto
                 {
                     Id = mt.Id,
                     UserId = mt.UserId,
                     Amount = amount,
                     IsApplied = isApplied,
                     TransactionId = linkedTransaction?.Id,
                     Notes = mt.Notes,
                     DayOfMonth = mt.DayOfMonth,
                     Name = mt.Notes, // Assuming Notes as name for monthly trans seems typical or use another field if available
                     MonthlyTransactionType = mt.MonthlyTransactionType,
                     AccountId = mt.AccountId,
                     AccountName = mt.AccountName,
                     CategoryId = mt.CategoryId,
                     CategoryName = mt.CategoryName,
                     Subcategory = mt.Subcategory,
                     CreatedAt = mt.CreatedAt,
                     UpdatedAt = mt.UpdatedAt,
                     Type = "monthlyTransaction"
                 };
            })
            .ToList();

        dto.IncomesAfterMonthlyExpenses.MonthlyIncomes.Items = monthlyIncomes;
        dto.IncomesAfterMonthlyExpenses.MonthlyIncomes.Total = monthlyIncomes.Sum(x => x.Amount);

        // 3. Process Monthly Expenses
        var monthlyExpenses = monthlyTransactions
            .Where(mt => mt.MonthlyTransactionType == MonthlyTransactionType.Expense)
            .Select(mt => {
                 var linkedTransaction = transactions.FirstOrDefault(t => t.MonthlyKey == mt.Id);
                 var isApplied = linkedTransaction != null;
                 var amount = isApplied ? linkedTransaction!.Amount : mt.Amount;
                 
                 return new BudgetSectionItemDto
                 {
                     Id = mt.Id,
                     UserId = mt.UserId,
                     Amount = amount,
                     IsApplied = isApplied,
                     TransactionId = linkedTransaction?.Id,
                     Notes = mt.Notes,
                     DayOfMonth = mt.DayOfMonth,
                     Name = mt.Notes,
                     MonthlyTransactionType = mt.MonthlyTransactionType,
                     AccountId = mt.AccountId,
                     AccountName = mt.AccountName,
                     CategoryId = mt.CategoryId,
                     CategoryName = mt.CategoryName,
                     Subcategory = mt.Subcategory,
                     CreatedAt = mt.CreatedAt,
                     UpdatedAt = mt.UpdatedAt,
                     Type = "monthlyTransaction"
                 };
            })
            .ToList();

        dto.IncomesAfterMonthlyExpenses.MonthlyExpenses.Items = monthlyExpenses;
        dto.IncomesAfterMonthlyExpenses.MonthlyExpenses.Total = monthlyExpenses.Sum(x => x.Amount);

        // 4. Process Savings
        var savingItems = savings
            .Select(s => {
                var linkedTransaction = transactions.FirstOrDefault(t => t.SavingKey == s.Id);
                var isApplied = linkedTransaction != null;
                // For savings, if applied, use transaction amount, otherwise use AmountPerMonth (default)
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
            })
            .ToList();

        dto.IncomesAfterMonthlyExpenses.Savings.Items = savingItems;
        dto.IncomesAfterMonthlyExpenses.Savings.Total = savingItems.Sum(x => x.Amount);

        // 5. Process Actual Incomes (Transactions) - Filter out those already linked to monthly incomes
        var actualIncomes = transactions
            .Where(t => t.TransactionType == TransactionType.Income && string.IsNullOrEmpty(t.MonthlyKey))
            .Select(t => new BudgetSectionItemDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Amount = t.Amount,
                IsApplied = t.IsApplied,
                TransactionId = t.Id,
                Notes = t.Notes,
                DayOfMonth = t.Date.Day,
                Name = t.Notes,
                AccountId = t.AccountId,
                AccountName = t.AccountName,
                CategoryId = t.CategoryId,
                CategoryName = t.CategoryName,
                Subcategory = t.Subcategory,
                Icon = t.CategoryImage,
                Color = t.CategoryColor,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Type = "transaction"
            })
            .ToList();

        dto.IncomesAfterMonthlyExpenses.Incomes.Items = actualIncomes;
        dto.IncomesAfterMonthlyExpenses.Incomes.Total = actualIncomes.Sum(x => x.Amount);

        // Calculate grand total (optional, depending on requirements, usually incomes - expenses - savings)
        // The requested JSON structure has a top level "total" which usually represents the "Income After Fixed Expenses"
        // Formula: Total Monthly Incomes - Total Monthly Expenses - Total Savings
        
        dto.IncomesAfterMonthlyExpenses.Total = 
            monthlyIncomes.Sum(x => x.Amount) + 
            dto.IncomesAfterMonthlyExpenses.Incomes.Total - 
            dto.IncomesAfterMonthlyExpenses.MonthlyExpenses.Total - 
            dto.IncomesAfterMonthlyExpenses.Savings.Total;

        return dto;
    }
}
