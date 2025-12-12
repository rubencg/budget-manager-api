using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Infrastructure.Pagination;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetArchivedAccountsHandler : IRequestHandler<GetArchivedAccountsQuery, PagedResult<Account>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetArchivedAccountsHandler> _logger;

    public GetArchivedAccountsHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<GetArchivedAccountsHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<Account>> Handle(GetArchivedAccountsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting archived accounts for user {UserId} - Page: {PageNumber}, Size: {PageSize}, Sort: {SortBy} {SortDirection}",
            userId, request.Pagination.PageNumber, request.Pagination.PageSize, 
            request.Sort.SortBy, request.Sort.SortDirection);

        // Fetch all archived accounts
        var accounts = await _accountRepository.GetArchivedAccountsAsync(userId, cancellationToken);

        // Define allowed sort fields
        var sortFieldMap = new Dictionary<string, Func<Account, object>>
        {
            { "name", a => a.Name ?? string.Empty },
            { "accounttype", a => a.AccountType?.Name ?? string.Empty },
            { "balance", a => a.CurrentBalance },
            { "updatedat", a => a.UpdatedAt }
        };

        // Apply sorting and pagination (default: updatedAt descending)
        if (string.IsNullOrWhiteSpace(request.Sort.SortBy))
        {
            request.Sort.SortBy = "updatedAt";
            request.Sort.SortDirection = "desc";
        }

        var pagedResult = ApplySortingAndPagination(accounts, request.Pagination, request.Sort, sortFieldMap);

        _logger.LogInformation(
            "Returning {Count} archived accounts (page {PageNumber} of {TotalPages})",
            pagedResult.Data.Count, pagedResult.PageNumber, pagedResult.TotalPages);

        return pagedResult;
    }

    private PagedResult<Account> ApplySortingAndPagination(
        List<Account> items,
        PaginationParams paginationParams,
        SortParams sortParams,
        Dictionary<string, Func<Account, object>> sortFieldMap)
    {
        // Apply sorting
        var sortBy = sortParams.SortBy?.ToLower() ?? "updatedat";
        
        if (!sortFieldMap.ContainsKey(sortBy))
        {
            _logger.LogWarning("Invalid sort field '{SortBy}', using default 'updatedAt'", sortBy);
            sortBy = "updatedat";
        }

        var sortedItems = sortParams.IsDescending
            ? items.OrderByDescending(sortFieldMap[sortBy]).ToList()
            : items.OrderBy(sortFieldMap[sortBy]).ToList();

        // Apply pagination
        return PagedResult<Account>.Create(sortedItems, paginationParams.PageNumber, paginationParams.PageSize);
    }
}
