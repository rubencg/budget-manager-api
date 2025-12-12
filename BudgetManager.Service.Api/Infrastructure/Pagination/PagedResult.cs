namespace BudgetManager.Service.Infrastructure.Pagination;

/// <summary>
/// Generic paged result wrapper
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Create a paged result from a list of items
    /// </summary>
    public static PagedResult<T> Create(List<T> allItems, int pageNumber, int pageSize)
    {
        var totalItems = allItems.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        var items = allItems
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1,
            Data = items
        };
    }
}
