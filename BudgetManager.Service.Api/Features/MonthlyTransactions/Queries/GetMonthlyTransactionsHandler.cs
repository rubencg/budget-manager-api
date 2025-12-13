using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public class GetMonthlyTransactionsHandler : IRequestHandler<GetMonthlyTransactionsQuery, List<MonthlyTransaction>>
{
    private readonly IMonthlyTransactionRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyTransactionsHandler(
        IMonthlyTransactionRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<List<MonthlyTransaction>> Handle(GetMonthlyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        return await _repository.GetAllAsync(userId, cancellationToken);
    }
}
