using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;
using BudgetManager.Service.Services.Validation;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionValidationService _transactionValidationService;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTransactionHandler> _logger;

    public UpdateTransactionHandler(
        ITransactionRepository transactionRepository,
        ITransactionValidationService transactionValidationService,
        IAccountBalanceService accountBalanceService,
        ICurrentUserService currentUserService,
        ILogger<UpdateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionValidationService = transactionValidationService;
        _accountBalanceService = accountBalanceService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Transaction> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Updating transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        // GET OLD TRANSACTION
        var oldTransaction = await _transactionRepository.GetByIdAsync(
            request.TransactionId!,
            userId,
            cancellationToken);

        if (oldTransaction == null)
        {
            _logger.LogError(
                "Transaction {TransactionId} not found for user {UserId}",
                request.TransactionId!, userId);
            throw new InvalidOperationException($"Transaction {request.TransactionId!} not found");
        }

        // VALIDATIONS
        ValidateUpdateRules(oldTransaction, request);

        // Validate OLD account(s) still exist (needed for balance reversal)
        // Only check if the old transaction was applied (otherwise no balance to reverse)
        bool oldApplied = IsTransactionApplied(oldTransaction);
        if (oldApplied)
        {
            if (oldTransaction.TransactionType == TransactionType.Transfer)
            {
                // For transfers, validate both old FromAccount and ToAccount still exist
                await _transactionValidationService.ValidateAccountExistsForReversalAsync(
                    oldTransaction.FromAccountId!,
                    userId,
                    cancellationToken);

                await _transactionValidationService.ValidateAccountExistsForReversalAsync(
                    oldTransaction.ToAccountId!,
                    userId,
                    cancellationToken);
            }
            else
            {
                // For non-transfer transactions, validate old AccountId still exists
                await _transactionValidationService.ValidateAccountExistsForReversalAsync(
                    oldTransaction.AccountId,
                    userId,
                    cancellationToken);
            }
        }

        // Validate NEW account(s) exist
        if (request.TransactionType == TransactionType.Transfer)
        {
            // For transfers, validate both FromAccountId and ToAccountId
            _transactionValidationService.ValidateTransferRules(
                request.FromAccountId,
                request.ToAccountId);

            await _transactionValidationService.ValidateTransferAccountsAsync(
                request.FromAccountId!,
                request.ToAccountId!,
                userId,
                cancellationToken);
        }
        else
        {
            // For non-transfer transactions, validate AccountId
            await _transactionValidationService.ValidateAccountExistsAsync(
                request.AccountId!,
                userId,
                cancellationToken);
        }

        // UPDATE TRANSACTION
        var updatedTransaction = new Transaction
        {
            Id = request.TransactionId!,
            UserId = userId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Date = request.Date,
            CreatedAt = oldTransaction.CreatedAt, // Preserve original creation date

            // Account references
            // For ALL transaction types, set AccountId and AccountName
            AccountId = request.AccountId!,
            AccountName = request.AccountName!,

            // For TRANSFERS ONLY, also set FromAccountId/ToAccountId
            FromAccountId = request.TransactionType == TransactionType.Transfer
                ? request.FromAccountId
                : null,
            FromAccountName = request.TransactionType == TransactionType.Transfer
                ? request.FromAccountName
                : null,
            ToAccountId = request.TransactionType == TransactionType.Transfer
                ? request.ToAccountId
                : null,
            ToAccountName = request.TransactionType == TransactionType.Transfer
                ? request.ToAccountName
                : null,

            // Category references
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            CategoryImage = request.CategoryImage,
            CategoryColor = request.CategoryColor,
            Subcategory = request.Subcategory,

            // Transaction details
            Notes = request.Notes,
            IsApplied = request.IsApplied,
            AppliedAmount = request.AppliedAmount,

            // Monthly/recurring
            IsMonthly = request.IsMonthly,
            IsRecurring = request.IsRecurring,
            MonthlyKey = request.MonthlyKey,
            RecurringTimes = request.RecurringTimes,
            RecurringType = request.RecurringType,

            // References
            SavingKey = request.SavingKey,
            TransferId = request.TransferId,
            RemoveFromSpendingPlan = request.RemoveFromSpendingPlan,

            // Recalculate indexing fields from new date
            YearMonth = $"{request.Date.Year}-{request.Date.Month:D2}",
            Year = request.Date.Year,
            Month = request.Date.Month,
            Day = request.Date.Day,

            Metadata = request.Metadata
        };

        var result = await _transactionRepository.UpdateAsync(updatedTransaction, cancellationToken);

        _logger.LogInformation(
            "Successfully updated transaction {TransactionId} for user {UserId}",
            request.TransactionId!, userId);

        // HANDLE BALANCE UPDATES
        await UpdateBalancesAsync(oldTransaction, updatedTransaction, userId, cancellationToken);

        return result;
    }

    /// <summary>
    /// Updates account balances based on old and new transaction states.
    /// Handles all combinations: old applied + new applied, old pending + new applied, etc.
    /// </summary>
    private async Task UpdateBalancesAsync(
        Transaction oldTransaction,
        Transaction newTransaction,
        string userId,
        CancellationToken cancellationToken)
    {
        bool oldApplied = IsTransactionApplied(oldTransaction);
        bool newApplied = IsTransactionApplied(newTransaction);

        _logger.LogInformation(
            "Updating balances for transaction {TransactionId}: OldApplied={OldApplied}, NewApplied={NewApplied}",
            newTransaction.Id, oldApplied, newApplied);

        // Reverse old if was applied
        if (oldApplied)
        {
            await _accountBalanceService.ReverseTransactionFromBalanceAsync(
                oldTransaction, userId, cancellationToken);

            _logger.LogInformation(
                "Reversed old transaction {TransactionId} from balance",
                oldTransaction.Id);
        }

        // Apply new if is applied
        if (newApplied)
        {
            await _accountBalanceService.ApplyTransactionToBalanceAsync(
                newTransaction, userId, cancellationToken);

            _logger.LogInformation(
                "Applied new transaction {TransactionId} to balance",
                newTransaction.Id);
        }
    }

    /// <summary>
    /// Determines if a transaction is applied to account balance.
    /// Transfers ALWAYS apply (no IsApplied check).
    /// Other types only apply if IsApplied == true.
    /// </summary>
    private bool IsTransactionApplied(Transaction transaction)
    {
        // Transfers always apply
        if (transaction.TransactionType == TransactionType.Transfer)
            return true;

        // Other types only apply if IsApplied = true
        return transaction.IsApplied;
    }

    /// <summary>
    /// Validates update-specific business rules.
    /// </summary>
    private void ValidateUpdateRules(Transaction oldTransaction, UpdateTransactionCommand request)
    {
        // Cannot change IsApplied from true to false
        if (oldTransaction.IsApplied && !request.IsApplied)
        {
            _logger.LogError(
                "Attempted to change IsApplied from true to false for transaction {TransactionId}",
                oldTransaction.Id);
            throw new InvalidOperationException(
                "Cannot change IsApplied from true to false. Delete and recreate the transaction instead.");
        }

        // Cannot change transaction type when applied
        if (oldTransaction.IsApplied && oldTransaction.TransactionType != request.TransactionType)
        {
            _logger.LogError(
                "Attempted to change transaction type from {OldType} to {NewType} when IsApplied=true for transaction {TransactionId}",
                oldTransaction.TransactionType, request.TransactionType, oldTransaction.Id);
            throw new InvalidOperationException(
                "Cannot change transaction type when IsApplied is true. Delete and recreate the transaction instead.");
        }

        // Cannot change to/from Transfer type when applied
        if (oldTransaction.IsApplied)
        {
            bool oldIsTransfer = oldTransaction.TransactionType == TransactionType.Transfer;
            bool newIsTransfer = request.TransactionType == TransactionType.Transfer;

            if (oldIsTransfer != newIsTransfer)
            {
                _logger.LogError(
                    "Attempted to change to/from Transfer type when IsApplied=true for transaction {TransactionId}",
                    oldTransaction.Id);
                throw new InvalidOperationException(
                    "Cannot change to/from Transfer type when transaction is applied. Delete and recreate the transaction instead.");
            }
        }
    }

}
