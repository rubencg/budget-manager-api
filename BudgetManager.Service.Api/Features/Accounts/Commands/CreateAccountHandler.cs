using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Account>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountHandler> _logger;

    public CreateAccountHandler(
        IAccountRepository accountRepository,
        ILogger<CreateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Account> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating account {AccountName} for user {UserId}",
            request.Name,
            request.UserId);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            Name = request.Name,
            CurrentBalance = request.CurrentBalance,
            AccountType = request.AccountType,
            Color = request.Color,
            Image = request.Image,
            IsArchived = request.IsArchived
        };

        return await _accountRepository.CreateAsync(account, cancellationToken);
    }
}
