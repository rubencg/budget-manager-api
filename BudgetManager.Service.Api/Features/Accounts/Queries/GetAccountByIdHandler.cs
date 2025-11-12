using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, Account?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountByIdHandler> _logger;

    public GetAccountByIdHandler(
        IAccountRepository accountRepository,
        ILogger<GetAccountByIdHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting account {AccountId} for user {UserId}",
            request.AccountId,
            request.UserId);

        return await _accountRepository.GetByIdAsync(
            request.AccountId,
            request.UserId,
            cancellationToken);
    }
}
