using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTransactionHandler> _logger;

    public DeleteTransactionHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        await _transactionRepository.DeleteAsync(
            request.TransactionId,
            userId,
            cancellationToken);

        _logger.LogInformation(
            "Successfully deleted transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        return Unit.Value;
    }
}
