using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Services.AccountBalance;

/// <summary>
/// Service responsible for synchronizing account balances with transaction operations.
/// Handles the application and reversal of transaction effects on account balances.
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// Applies a transaction's effect to the account balance(s).
    /// For expenses: deducts from account.
    /// For income: adds to account.
    /// For transfers: deducts from source account and adds to destination account.
    /// Note: Transfers always apply immediately regardless of IsApplied flag.
    /// </summary>
    /// <param name="transaction">The transaction to apply</param>
    /// <param name="userId">The user ID (for security validation)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when account is not found or ToAccountId is missing for transfers</exception>
    Task ApplyTransactionToBalanceAsync(
        Transaction transaction,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a transaction's effect from the account balance(s).
    /// This is the inverse of ApplyTransactionToBalance.
    /// Used when deleting transactions or updating from IsApplied=true to IsApplied=false.
    /// For expenses: adds back to account (reverses deduction).
    /// For income: deducts from account (reverses addition).
    /// For transfers: adds back to source account and deducts from destination account.
    /// </summary>
    /// <param name="transaction">The transaction to reverse</param>
    /// <param name="userId">The user ID (for security validation)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when account is not found or ToAccountId is missing for transfers</exception>
    Task ReverseTransactionFromBalanceAsync(
        Transaction transaction,
        string userId,
        CancellationToken cancellationToken = default);
}
