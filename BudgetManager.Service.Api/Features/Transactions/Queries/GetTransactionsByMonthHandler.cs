using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Infrastructure.Pagination;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionsByMonthQueryHandler
    : IRequestHandler<GetTransactionsByMonthQuery, PagedResult<Transaction>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTransactionsByMonthQueryHandler> _logger;

    public GetTransactionsByMonthQueryHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<GetTransactionsByMonthQueryHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<Transaction>> Handle(
        GetTransactionsByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var yearMonth = $"{request.Year}-{request.Month:D2}";

        _logger.LogInformation(
            "Getting transactions for {YearMonth}, user {UserId} - Page: {PageNumber}, Size: {PageSize}",
            yearMonth, userId, request.Pagination.PageNumber, request.Pagination.PageSize);

        var transactions = await _transactionRepository.GetByMonthAsync(
            userId, 
            yearMonth,
            request.TransactionType,
            categoryId: request.CategoryId,
            cancellationToken: cancellationToken);

        // Define allowed sort fields
        var sortFieldMap = new Dictionary<string, Func<Transaction, object>>
        {
            { "date", t => t.Date },
            { "amount", t => t.Amount },
            { "accountname", t => t.AccountName ?? string.Empty },
            { "categoryname", t => t.CategoryName ?? string.Empty },
            { "transactiontype", t => t.TransactionType }
        };

        // Apply sorting and pagination (default: date descending)
        if (string.IsNullOrWhiteSpace(request.Sort.SortBy))
        {
            request.Sort.SortBy = "date";
            request.Sort.SortDirection = "desc";
        }

        var pagedResult = ApplySortingAndPagination(transactions, request.Pagination, request.Sort, sortFieldMap);

        _logger.LogInformation(
            "Returning {Count} transactions for {YearMonth} (page {PageNumber} of {TotalPages})",
            pagedResult.Data.Count, yearMonth, pagedResult.PageNumber, pagedResult.TotalPages);

        return pagedResult;
    }

    private PagedResult<Transaction> ApplySortingAndPagination(
        List<Transaction> items,
        PaginationParams paginationParams,
        SortParams sortParams,
        Dictionary<string, Func<Transaction, object>> sortFieldMap)
    {
        // Apply sorting
        var sortBy = sortParams.SortBy?.ToLower() ?? "date";
        
        if (!sortFieldMap.ContainsKey(sortBy))
        {
            _logger.LogWarning("Invalid sort field '{SortBy}', using default 'date'", sortBy);
            sortBy = "date";
        }

        var sortedItems = sortParams.IsDescending
            ? items.OrderByDescending(sortFieldMap[sortBy]).ToList()
            : items.OrderBy(sortFieldMap[sortBy]).ToList();

        // Apply pagination
        return PagedResult<Transaction>.Create(sortedItems, paginationParams.PageNumber, paginationParams.PageSize);
    }
}