using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;

namespace BudgetManager.Service.Services.AccountBalance;

/// <inheritdoc />
public class AccountBalanceService : IAccountBalanceService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountBalanceService> _logger;

    public AccountBalanceService(
        IAccountRepository accountRepository,
        ILogger<AccountBalanceService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ApplyTransactionToBalanceAsync(
        Transaction transaction,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Applying transaction {TransactionId} of type {TransactionType} to account balance for user {UserId}",
            transaction.Id, transaction.TransactionType, userId);

        switch (transaction.TransactionType)
        {
            case TransactionType.Expense:
            case TransactionType.MonthlyExpense:
                await DeductFromAccountAsync(
                    transaction.AccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            case TransactionType.Income:
            case TransactionType.MonthlyIncome:
                await AddToAccountAsync(
                    transaction.AccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            case TransactionType.Transfer:
                if (string.IsNullOrEmpty(transaction.FromAccountId))
                {
                    _logger.LogError(
                        "Transfer transaction {TransactionId} is missing FromAccountId",
                        transaction.Id);
                    throw new InvalidOperationException(
                        $"Transfer transaction {transaction.Id} must have a source account (FromAccountId)");
                }

                if (string.IsNullOrEmpty(transaction.ToAccountId))
                {
                    _logger.LogError(
                        "Transfer transaction {TransactionId} is missing ToAccountId",
                        transaction.Id);
                    throw new InvalidOperationException(
                        $"Transfer transaction {transaction.Id} must have a destination account (ToAccountId)");
                }

                // Deduct from source account
                await DeductFromAccountAsync(
                    transaction.FromAccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);

                // Add to destination account
                await AddToAccountAsync(
                    transaction.ToAccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            default:
                _logger.LogError(
                    "Unknown transaction type {TransactionType} for transaction {TransactionId}",
                    transaction.TransactionType, transaction.Id);
                throw new InvalidOperationException(
                    $"Unknown transaction type: {transaction.TransactionType}");
        }

        _logger.LogInformation(
            "Successfully applied transaction {TransactionId} to account balance(s)",
            transaction.Id);
    }

    /// <inheritdoc />
    public async Task ReverseTransactionFromBalanceAsync(
        Transaction transaction,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Reversing transaction {TransactionId} of type {TransactionType} from account balance for user {UserId}",
            transaction.Id, transaction.TransactionType, userId);

        switch (transaction.TransactionType)
        {
            case TransactionType.Expense:
            case TransactionType.MonthlyExpense:
                // Reverse expense = add back to account
                await AddToAccountAsync(
                    transaction.AccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            case TransactionType.Income:
            case TransactionType.MonthlyIncome:
                // Reverse income = deduct from account
                await DeductFromAccountAsync(
                    transaction.AccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            case TransactionType.Transfer:
                if (string.IsNullOrEmpty(transaction.FromAccountId))
                {
                    _logger.LogError(
                        "Transfer transaction {TransactionId} is missing FromAccountId",
                        transaction.Id);
                    throw new InvalidOperationException(
                        $"Transfer transaction {transaction.Id} must have a source account (FromAccountId)");
                }

                if (string.IsNullOrEmpty(transaction.ToAccountId))
                {
                    _logger.LogError(
                        "Transfer transaction {TransactionId} is missing ToAccountId",
                        transaction.Id);
                    throw new InvalidOperationException(
                        $"Transfer transaction {transaction.Id} must have a destination account (ToAccountId)");
                }

                // Reverse transfer: add back to source account
                await AddToAccountAsync(
                    transaction.FromAccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);

                // Reverse transfer: deduct from destination account
                await DeductFromAccountAsync(
                    transaction.ToAccountId,
                    transaction.Amount,
                    userId,
                    transaction.TransactionType,
                    cancellationToken);
                break;

            default:
                _logger.LogError(
                    "Unknown transaction type {TransactionType} for transaction {TransactionId}",
                    transaction.TransactionType, transaction.Id);
                throw new InvalidOperationException(
                    $"Unknown transaction type: {transaction.TransactionType}");
        }

        _logger.LogInformation(
            "Successfully reversed transaction {TransactionId} from account balance(s)",
            transaction.Id);
    }

    /// <summary>
    /// Deducts an amount from an account's current balance.
    /// </summary>
    /// <param name="accountId">The account ID to deduct from</param>
    /// <param name="amount">The amount to deduct</param>
    /// <param name="userId">The user ID for security validation</param>
    /// <param name="transactionType">The transaction type for logging purposes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when account is not found</exception>
    private async Task DeductFromAccountAsync(
        string accountId,
        decimal amount,
        string userId,
        TransactionType transactionType,
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

        var oldBalance = account.CurrentBalance;
        account.CurrentBalance -= amount;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation(
            "Deducted {Amount} from account {AccountId} (Type: {TransactionType}). Old balance: {OldBalance}, New balance: {NewBalance}",
            amount, accountId, transactionType, oldBalance, account.CurrentBalance);
    }

    /// <summary>
    /// Adds an amount to an account's current balance.
    /// </summary>
    /// <param name="accountId">The account ID to add to</param>
    /// <param name="amount">The amount to add</param>
    /// <param name="userId">The user ID for security validation</param>
    /// <param name="transactionType">The transaction type for logging purposes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when account is not found</exception>
    private async Task AddToAccountAsync(
        string accountId,
        decimal amount,
        string userId,
        TransactionType transactionType,
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

        var oldBalance = account.CurrentBalance;
        account.CurrentBalance += amount;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation(
            "Added {Amount} to account {AccountId} (Type: {TransactionType}). Old balance: {OldBalance}, New balance: {NewBalance}",
            amount, accountId, transactionType, oldBalance, account.CurrentBalance);
    }
}
