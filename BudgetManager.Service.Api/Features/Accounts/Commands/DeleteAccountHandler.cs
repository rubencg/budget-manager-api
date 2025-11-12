using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        IAccountRepository accountRepository,
        ILogger<DeleteAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting account {AccountId} for user {UserId}",
            request.AccountId,
            request.UserId);

        await _accountRepository.DeleteAsync(
            request.AccountId,
            request.UserId,
            cancellationToken);
    }
}
