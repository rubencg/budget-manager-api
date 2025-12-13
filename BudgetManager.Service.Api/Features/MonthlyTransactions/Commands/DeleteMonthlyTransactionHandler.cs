using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public class DeleteMonthlyTransactionHandler : IRequestHandler<DeleteMonthlyTransactionCommand>
{
    private readonly IMonthlyTransactionRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMonthlyTransactionHandler(
        IMonthlyTransactionRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteMonthlyTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        // Ensure it exists before deleting? Or just try deleting.
        // Usually good practice to valid ownership implicitly via userId in repository DeleteAsync
        await _repository.DeleteAsync(request.Id, userId, cancellationToken);
    }
}
