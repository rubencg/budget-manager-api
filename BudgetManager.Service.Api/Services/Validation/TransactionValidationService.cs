using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;

namespace BudgetManager.Service.Services.Validation;

/// <inheritdoc />
public class TransactionValidationService : ITransactionValidationService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<TransactionValidationService> _logger;

    public TransactionValidationService(
        IAccountRepository accountRepository,
        ILogger<TransactionValidationService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ValidateAccountExistsAsync(
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
                "Cannot create transaction for archived account {AccountId}",
                accountId);
            throw new InvalidOperationException(
                $"Cannot create transactions for archived account {account.Name}");
        }

        _logger.LogDebug(
            "Account {AccountId} validated successfully for user {UserId}",
            accountId, userId);
    }

    /// <inheritdoc />
    public void ValidateTransferRules(string? fromAccountId, string? toAccountId)
    {
        // FromAccountId is required for transfers
        if (string.IsNullOrEmpty(fromAccountId))
        {
            _logger.LogError("Transfer transaction is missing FromAccountId");
            throw new InvalidOperationException("FromAccountId is required for transfer transactions");
        }

        // ToAccountId is required for transfers
        if (string.IsNullOrEmpty(toAccountId))
        {
            _logger.LogError("Transfer transaction is missing ToAccountId");
            throw new InvalidOperationException("ToAccountId is required for transfer transactions");
        }

        // Cannot transfer to same account
        if (fromAccountId == toAccountId)
        {
            _logger.LogError(
                "Transfer transaction has same source and destination account: {AccountId}",
                fromAccountId);
            throw new InvalidOperationException("Cannot transfer to the same account");
        }

        _logger.LogDebug(
            "Transfer rules validated successfully: FromAccountId={FromAccountId}, ToAccountId={ToAccountId}",
            fromAccountId, toAccountId);
    }

    /// <inheritdoc />
    public void ValidateMonthlyTransactionRules(TransactionType transactionType, bool isApplied)
    {
        if (transactionType == TransactionType.MonthlyExpense && isApplied)
        {
            _logger.LogError(
                "Attempted to create MonthlyExpense with IsApplied=true");
            throw new InvalidOperationException(
                "MonthlyExpense transactions must be created with IsApplied=false");
        }

        if (transactionType == TransactionType.MonthlyIncome && isApplied)
        {
            _logger.LogError(
                "Attempted to create MonthlyIncome with IsApplied=true");
            throw new InvalidOperationException(
                "MonthlyIncome transactions must be created with IsApplied=false");
        }

        _logger.LogDebug(
            "Monthly transaction rules validated successfully for type {TransactionType}",
            transactionType);
    }

    /// <inheritdoc />
    public async Task ValidateTransferAccountsAsync(
        string fromAccountId,
        string toAccountId,
        string userId,
        CancellationToken cancellationToken)
    {
        // Validate source account (FromAccountId)
        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId, userId, cancellationToken);

        if (fromAccount == null)
        {
            _logger.LogError(
                "Source account {AccountId} not found for user {UserId}",
                fromAccountId, userId);
            throw new InvalidOperationException($"Source account {fromAccountId} not found");
        }

        if (fromAccount.IsArchived)
        {
            _logger.LogError(
                "Cannot transfer from archived account {AccountId}",
                fromAccountId);
            throw new InvalidOperationException(
                $"Cannot transfer from archived account {fromAccount.Name}");
        }

        // Validate destination account (ToAccountId)
        var toAccount = await _accountRepository.GetByIdAsync(toAccountId, userId, cancellationToken);

        if (toAccount == null)
        {
            _logger.LogError(
                "Destination account {AccountId} not found for user {UserId}",
                toAccountId, userId);
            throw new InvalidOperationException($"Destination account {toAccountId} not found");
        }

        if (toAccount.IsArchived)
        {
            _logger.LogError(
                "Cannot transfer to archived account {AccountId}",
                toAccountId);
            throw new InvalidOperationException(
                $"Cannot transfer to archived account {toAccount.Name}");
        }

        _logger.LogDebug(
            "Transfer accounts validated successfully: FromAccountId={FromAccountId}, ToAccountId={ToAccountId}",
            fromAccountId, toAccountId);
    }

    /// <inheritdoc />
    public async Task ValidateAccountExistsForReversalAsync(
        string accountId,
        string userId,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, userId, cancellationToken);

        if (account == null)
        {
            _logger.LogError(
                "Account {AccountId} not found for user {UserId} during balance reversal validation",
                accountId, userId);
            throw new InvalidOperationException(
                $"Cannot update transaction: original account {accountId} no longer exists. " +
                "The account may have been deleted. Please contact support if you need to update this transaction.");
        }

        // Note: We allow archived accounts here because we need to reverse balances
        // even if the account was archived after the transaction was created

        _logger.LogDebug(
            "Account {AccountId} validated successfully for reversal for user {UserId}",
            accountId, userId);
    }
}
