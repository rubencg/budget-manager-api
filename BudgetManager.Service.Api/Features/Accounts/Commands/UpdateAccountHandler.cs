using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand, Account>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateAccountHandler> _logger;

    public UpdateAccountHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Account> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Updating account {AccountId} for user {UserId}",
            request.AccountId,
            userId);

        // Get existing account to preserve CreatedAt
        var existingAccount = await _accountRepository.GetByIdAsync(
            request.AccountId,
            userId,
            cancellationToken);

        if (existingAccount == null)
        {
            throw new InvalidOperationException($"Account {request.AccountId} not found");
        }

        // Update account with new values
        existingAccount.Name = request.Name;
        existingAccount.CurrentBalance = request.CurrentBalance;
        existingAccount.AccountType = request.AccountType;
        existingAccount.Color = request.Color;
        existingAccount.IsArchived = request.IsArchived;
        existingAccount.Image = request.Image;

        return await _accountRepository.UpdateAsync(existingAccount, cancellationToken);
    }
}
