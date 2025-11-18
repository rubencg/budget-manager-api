using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTransactionHandler> _logger;

    public DeleteTransactionHandler(
        ITransactionRepository transactionRepository,
        IAccountBalanceService accountBalanceService,
        ICurrentUserService currentUserService,
        ILogger<DeleteTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _accountBalanceService = accountBalanceService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        // FETCH TRANSACTION (to get its state before deletion)
        var transaction = await _transactionRepository.GetByIdAsync(
            request.TransactionId,
            userId,
            cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning(
                "Transaction {TransactionId} not found for user {UserId}",
                request.TransactionId, userId);
            return Unit.Value; // Already deleted or never existed
        }

        // REVERSE BALANCE IF APPLIED
        if (ShouldReverseBalance(transaction))
        {
            await _accountBalanceService.ReverseTransactionFromBalanceAsync(
                transaction,
                userId,
                cancellationToken);

            _logger.LogInformation(
                "Reversed balance for transaction {TransactionId} of type {TransactionType}",
                transaction.Id, transaction.TransactionType);
        }

        // DELETE TRANSACTION
        await _transactionRepository.DeleteAsync(
            request.TransactionId,
            userId,
            cancellationToken);

        _logger.LogInformation(
            "Successfully deleted transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        return Unit.Value;
    }

    /// <summary>
    /// Determines if a transaction's balance should be reversed when deleting.
    /// Transfers ALWAYS need reversal (they always apply).
    /// Other types only need reversal if IsApplied == true.
    /// </summary>
    private bool ShouldReverseBalance(BudgetManager.Api.Domain.Entities.Transaction transaction)
    {
        // Transfers always apply, so always reverse
        if (transaction.TransactionType == TransactionType.Transfer)
            return true;

        // Other types only reverse if was applied
        return transaction.IsApplied;
    }
}
