using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetAccountsDashboardHandler : IRequestHandler<GetAccountsDashboardQuery, List<AccountDashboardGroupDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAccountsDashboardHandler> _logger;

    public GetAccountsDashboardHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<GetAccountsDashboardHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<List<AccountDashboardGroupDto>> Handle(GetAccountsDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation("Getting dashboard accounts for user {UserId}", userId);

        var accounts = await _accountRepository.GetDashboardAccountsAsync(userId, cancellationToken);

        var groupedAccounts = accounts
            .GroupBy(a => a.AccountType.Name)
            .Select(g => new AccountDashboardGroupDto
            {
                GroupName = g.Key,
                Total = g.Sum(a => a.CurrentBalance),
                Accounts = g.ToList()
            })
            .ToList();

        return groupedAccounts;
    }
}
