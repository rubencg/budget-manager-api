using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting account {AccountId} for user {UserId}",
            request.AccountId,
            userId);

        await _accountRepository.DeleteAsync(
            request.AccountId,
            userId,
            cancellationToken);
    }
}
