using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Api.Features.Budget.DTOs;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Api.Features.Budget.Queries;

public class GetOtherExpensesQueryHandler : IRequestHandler<GetOtherExpensesQuery, OtherExpensesResponseDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;

    public GetOtherExpensesQueryHandler(
        ITransactionRepository transactionRepository,
        IPlannedExpenseRepository plannedExpenseRepository)
    {
        _transactionRepository = transactionRepository;
        _plannedExpenseRepository = plannedExpenseRepository;
    }

    public async Task<OtherExpensesResponseDto> Handle(GetOtherExpensesQuery request, CancellationToken cancellationToken)
    {
        var yearMonth = $"{request.Year}-{request.Month:D2}";

        // 1. Fetch transactions for the month
        var transactions = await _transactionRepository.GetByMonthAsync(request.UserId, yearMonth, cancellationToken: cancellationToken);

        // 2. Fetch all planned expenses for the user
        var allPlannedExpenses = await _plannedExpenseRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // 3. Filter valid PEs for this month
        var activePlannedExpenses = allPlannedExpenses.Where(pe =>
            pe.IsRecurring ||
            (pe.Date.HasValue && pe.Date.Value.Year == request.Year && pe.Date.Value.Month == request.Month)
        ).ToList();

        // 4. Filter transactions for "Other Expenses"
        // Exclude:
        // - Transfers, Incomes (Only want Expenses)
        // - Monthly transactions (MonthlyKey != null)
        // - Savings transactions (SavingKey != null)
        // - RemoveFromSpendingPlan == true
        // - Planned Expenses (category/subcategory match with an active PE)
        var otherExpenses = transactions.Where(t =>
            t.TransactionType == TransactionType.Expense &&
            string.IsNullOrEmpty(t.MonthlyKey) &&
            string.IsNullOrEmpty(t.SavingKey) &&
            !t.RemoveFromSpendingPlan &&
            !activePlannedExpenses.Any(pe =>
                t.CategoryId == pe.CategoryId &&
                (string.IsNullOrEmpty(pe.SubCategory) || t.Subcategory == pe.SubCategory)
            )
        ).ToList();

        var result = new OtherExpensesResponseDto
        {
            Items = otherExpenses
                .OrderByDescending(t => t.Date)
                .Select(t => new BudgetSectionItemDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    IsApplied = t.IsApplied,
                    TransactionId = t.Id,
                    Notes = t.Notes,
                    Icon = t.CategoryImage,
                    Color = t.CategoryColor,
                    Name = t.CategoryName,
                    CategoryId = t.CategoryId,
                    CategoryName = t.CategoryName,
                    Subcategory = t.Subcategory,
                    AccountId = t.AccountId,
                    AccountName = t.AccountName,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Type = "transaction"
                })
                .ToList()
        };

        result.Total = result.Items.Sum(x => x.Amount);

        return result;
    }
}
