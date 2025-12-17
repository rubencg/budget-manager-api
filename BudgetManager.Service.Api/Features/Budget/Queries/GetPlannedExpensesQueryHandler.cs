using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Api.Features.Budget.DTOs;
using BudgetManager.Service.Features.PlannedExpenses.Queries;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Api.Features.Budget.Queries;

public class GetPlannedExpensesQueryHandler : IRequestHandler<GetPlannedExpensesQuery, PlannedExpensesResponseDto>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetPlannedExpensesQueryHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ITransactionRepository transactionRepository)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PlannedExpensesResponseDto> Handle(GetPlannedExpensesQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch all planned expenses for the user
        var allPlannedExpenses = await _plannedExpenseRepository.GetByUserIdAsync(request.UserId);

        // 2. Filter valid PEs (Recurring or matching Month/Year)
        // Note: For recurring expenses, we might need additional logic to check if they are active for this month
        // For now assuming all recurring are valid, or filtered by creation date logic if requested.
        // The requirements say "recurring and for the current month".
        // Assuming strict "Date" matching for non-recurring.
        var targetDate = new DateTime(request.Year, request.Month, 1);
        var activePlannedExpenses = allPlannedExpenses.Where(pe =>
            pe.IsRecurring ||
            (pe.Date.HasValue && pe.Date.Value.Year == request.Year && pe.Date.Value.Month == request.Month)
        ).ToList();

        // 3. Fetch all transactions for the month
        // 3. Fetch all transactions for the month
        var yearMonth = $"{request.Year}-{request.Month:D2}";
        var transactions = await _transactionRepository.GetByMonthAsync(request.UserId, yearMonth);
        
        // Filter out non-expense transactions if necessary, or just use them all for matching
        // Usually planned expenses track 'Expenses', so we might filter TransactionType == Expense
        // But let's keep all and match by category as requested.

        var result = new PlannedExpensesResponseDto();
        var plannedExpenseDtos = new List<PlannedExpenseViewDto>();

        foreach (var pe in activePlannedExpenses)
        {
            // 4. Calculate stats for each PE
            // Match transaction by CategoryId
            // And Subcategory if PE has one.
            var matchingTransactions = transactions.Where(t =>
                t.CategoryId == pe.CategoryId
                && (string.IsNullOrEmpty(pe.SubCategory) || t.Subcategory == pe.SubCategory)
                && t.TransactionType == TransactionType.Expense // Assuming we only track expenses against planned expenses
            ).ToList();

            var amountSpent = matchingTransactions.Sum(t => t.Amount);
            var amountLeft = pe.TotalAmount - amountSpent;
            var percentageSpent = pe.TotalAmount > 0 ? (amountSpent / pe.TotalAmount) * 100 : 0;

            var dto = new PlannedExpenseViewDto
            {
                Id = pe.Id,
                UserId = pe.UserId,
                ItemType = pe.ItemType,
                Name = pe.Name,
                Date = pe.Date,
                DayOfMonth = pe.DayOfMonth,
                IsRecurring = pe.IsRecurring,
                TotalAmount = pe.TotalAmount,
                CategoryId = pe.CategoryId,
                CategoryName = pe.CategoryName,
                CategoryImage = pe.CategoryImage,
                CategoryColor = pe.CategoryColor,
                SubCategory = pe.SubCategory,
                CreatedAt = pe.CreatedAt,
                UpdatedAt = pe.UpdatedAt
            };
            dto.AmountSpent = amountSpent;
            dto.AmountLeft = amountLeft;
            dto.PercentageSpent = percentageSpent;
            
            plannedExpenseDtos.Add(dto);
        }

        result.PlannedExpenses = plannedExpenseDtos;

        // 5. Calculate overall Total
        // User requirement: sum of the max between totalAmount or the sum of the expenses for that planned expense.
        result.Total = plannedExpenseDtos.Sum(pe => Math.Max(pe.TotalAmount, pe.AmountSpent));


        // 6. Filter Items (Transactions) to return
        IEnumerable<Transaction> toReturn;

        if (!string.IsNullOrEmpty(request.PlannedExpenseId))
        {
            // Return all items that match THE category/subcategory for the sent planned expense
            var specificPe = activePlannedExpenses.FirstOrDefault(pe => pe.Id == request.PlannedExpenseId);
            if (specificPe != null)
            {
                toReturn = transactions.Where(t =>
                    t.CategoryId == specificPe.CategoryId
                    && (string.IsNullOrEmpty(specificPe.SubCategory) || t.Subcategory == specificPe.SubCategory)
                    && t.TransactionType == TransactionType.Expense
                );
            }
            else
            {
                toReturn = Enumerable.Empty<Transaction>();
            }
        }
        else
        {
            // Return all items that match ANY of the categories/subcategories for the current's date planned expenses
            // We can collect all matching transactions from the loop above or re-filter.
            // Let's re-filter to be clean.
            toReturn = transactions.Where(t => activePlannedExpenses.Any(pe =>
                t.CategoryId == pe.CategoryId
                && (string.IsNullOrEmpty(pe.SubCategory) || t.Subcategory == pe.SubCategory)
                && t.TransactionType == TransactionType.Expense
            ));
        }

        result.Items = toReturn
            .OrderByDescending(t => t.Date)
            .Select(t => new BudgetSectionItemDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Amount = t.Amount,
                IsApplied = t.IsApplied,
                Notes = t.Notes,
                Icon = t.CategoryImage,
                Color = t.CategoryColor, // Or verify if color matches 
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
            .ToList();

        return result;
    }
}
