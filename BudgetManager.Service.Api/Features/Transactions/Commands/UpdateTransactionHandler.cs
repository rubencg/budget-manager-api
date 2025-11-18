using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTransactionHandler> _logger;

    public UpdateTransactionHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAccountBalanceService accountBalanceService,
        ICurrentUserService currentUserService,
        ILogger<UpdateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
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
        await ValidateAccountExistsAsync(request.AccountId, userId, cancellationToken);
        ValidateTransferRules(request);

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
            AccountId = request.AccountId,
            AccountName = request.AccountName,
            ToAccountId = request.ToAccountId,
            ToAccountName = request.ToAccountName,

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

    /// <summary>
    /// Validates that the account exists and is not archived.
    /// </summary>
    private async Task ValidateAccountExistsAsync(
        string accountId,
        string userId,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, userId, cancellationToken);

        if (account == null)
        {
            _logger.LogError(
                "Account {AccountId} not found for user {UserId}",
                accountId, userId);
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        if (account.IsArchived)
        {
            _logger.LogError(
                "Cannot update transaction with archived account {AccountId}",
                accountId);
            throw new InvalidOperationException(
                $"Cannot use archived account {account.Name} for transactions");
        }
    }

    /// <summary>
    /// Validates transfer-specific business rules.
    /// </summary>
    private void ValidateTransferRules(UpdateTransactionCommand request)
    {
        if (request.TransactionType != TransactionType.Transfer)
            return;

        // ToAccountId is required for transfers
        if (string.IsNullOrEmpty(request.ToAccountId))
        {
            _logger.LogError("Transfer transaction is missing ToAccountId");
            throw new InvalidOperationException("ToAccountId is required for transfer transactions");
        }

        // Cannot transfer to same account
        if (request.AccountId == request.ToAccountId)
        {
            _logger.LogError(
                "Transfer transaction has same source and destination account: {AccountId}",
                request.AccountId);
            throw new InvalidOperationException("Cannot transfer to the same account");
        }

        // Note: We don't validate ToAccountId existence here because we want to keep
        // the validation lightweight. The AccountBalanceService will validate this
        // when applying the balance update.
    }
}
