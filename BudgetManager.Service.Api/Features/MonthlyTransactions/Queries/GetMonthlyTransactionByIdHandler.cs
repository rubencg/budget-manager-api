using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Queries;

public class GetMonthlyTransactionByIdHandler : IRequestHandler<GetMonthlyTransactionByIdQuery, MonthlyTransaction?>
{
    private readonly IMonthlyTransactionRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyTransactionByIdHandler(
        IMonthlyTransactionRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlyTransaction?> Handle(GetMonthlyTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        return await _repository.GetByIdAsync(request.Id, userId, cancellationToken);
    }
}
