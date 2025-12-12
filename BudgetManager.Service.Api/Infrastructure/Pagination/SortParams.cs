namespace BudgetManager.Service.Infrastructure.Pagination;

/// <summary>
/// Reusable sorting parameters
/// </summary>
public class SortParams
{
    /// <summary>
    /// Field name to sort by
    /// </summary>
    public string SortBy { get; set; } = string.Empty;

    /// <summary>
    /// Sort direction: "asc" or "desc"
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Check if sorting is descending
    /// </summary>
    public bool IsDescending => SortDirection?.ToLower() == "desc";
}
