using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public class GetOverdueMonthlyTransactionsHandler : IRequestHandler<GetOverdueMonthlyTransactionsQuery, List<MonthlyTransaction>>
{
    private readonly IMonthlyTransactionRepository _monthlyTransactionRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetOverdueMonthlyTransactionsHandler(
        IMonthlyTransactionRepository monthlyTransactionRepository,
        ITransactionRepository transactionRepository)
    {
        _monthlyTransactionRepository = monthlyTransactionRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<List<MonthlyTransaction>> Handle(GetOverdueMonthlyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var allMonthly = await _monthlyTransactionRepository.GetAllAsync(request.UserId, cancellationToken);

        var expenses = allMonthly.Where(mt => mt.MonthlyTransactionType == MonthlyTransactionType.Expense);

        var yearMonth = $"{request.Date.Year}-{request.Date.Month:D2}";
        var monthTransactions = await _transactionRepository.GetByMonthAsync(request.UserId, yearMonth, cancellationToken: cancellationToken);

        var appliedKeys = monthTransactions
            .Where(t => t.MonthlyKey != null)
            .Select(t => t.MonthlyKey!)
            .ToHashSet();

        return expenses
            .Where(mt => mt.DayOfMonth <= request.Date.Day && !appliedKeys.Contains(mt.Id))
            .OrderBy(mt => mt.DayOfMonth)
            .ToList();
    }
}
