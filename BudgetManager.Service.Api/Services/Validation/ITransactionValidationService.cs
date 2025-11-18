using BudgetManager.Api.Domain.Enums;

namespace BudgetManager.Service.Services.Validation;

/// <summary>
/// Service for validating transaction-related business rules.
/// Centralizes validation logic to eliminate duplication across handlers.
/// </summary>
public interface ITransactionValidationService
{
    /// <summary>
    /// Validates that a single account exists and is not archived.
    /// Used for non-transfer transactions (Expense, Income, MonthlyExpense, MonthlyIncome).
    /// </summary>
    /// <param name="accountId">The account ID to validate</param>
    /// <param name="userId">The user ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when account is not found or is archived</exception>
    Task ValidateAccountExistsAsync(string accountId, string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates transfer-specific business rules.
    /// Ensures FromAccountId and ToAccountId are provided and different.
    /// </summary>
    /// <param name="fromAccountId">The source account ID</param>
    /// <param name="toAccountId">The destination account ID</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
    void ValidateTransferRules(string? fromAccountId, string? toAccountId);

    /// <summary>
    /// Validates monthly transaction business rules.
    /// Ensures MonthlyExpense and MonthlyIncome are created with IsApplied=false.
    /// </summary>
    /// <param name="transactionType">The transaction type</param>
    /// <param name="isApplied">The IsApplied flag</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
    void ValidateMonthlyTransactionRules(TransactionType transactionType, bool isApplied);

    /// <summary>
    /// Validates that both transfer accounts exist and are not archived.
    /// Combines account existence validation for both source and destination accounts.
    /// </summary>
    /// <param name="fromAccountId">The source account ID</param>
    /// <param name="toAccountId">The destination account ID</param>
    /// <param name="userId">The user ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when either account is not found or is archived</exception>
    Task ValidateTransferAccountsAsync(string fromAccountId, string toAccountId, string userId, CancellationToken cancellationToken);
}
