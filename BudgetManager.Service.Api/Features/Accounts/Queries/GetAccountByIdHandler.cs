using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, Account?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAccountByIdHandler> _logger;

    public GetAccountByIdHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<GetAccountByIdHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting account {AccountId} for user {UserId}",
            request.AccountId,
            userId);

        return await _accountRepository.GetByIdAsync(
            request.AccountId,
            userId,
            cancellationToken);
    }
}
