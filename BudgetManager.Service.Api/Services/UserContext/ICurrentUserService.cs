namespace BudgetManager.Service.Services.UserContext;

/// <summary>
/// Service for accessing the current authenticated user's context information from JWT claims.
/// This service extracts user information from the HTTP context's authentication token.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The user's unique identifier extracted from the 'sub' or NameIdentifier claim.</returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the user is not authenticated or the user ID claim is not present in the token.
    /// </exception>
    string UserId { get; }

    /// <summary>
    /// Gets the email address of the currently authenticated user.
    /// </summary>
    /// <returns>The user's email address if present in the token claims; otherwise, null.</returns>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    /// <returns>true if the user is authenticated; otherwise, false.</returns>
    bool IsAuthenticated { get; }
}
