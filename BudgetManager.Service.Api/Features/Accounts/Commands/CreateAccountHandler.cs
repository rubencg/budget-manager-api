using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Account>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateAccountHandler> _logger;

    public CreateAccountHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Account> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating account {AccountName} for user {UserId}",
            request.Name,
            userId);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = request.Name,
            CurrentBalance = request.CurrentBalance,
            AccountType = request.AccountType,
            Color = request.Color,
            Image = request.Image,
            IsArchived = request.IsArchived,
            AvailableCredit = request.AvailableCredit
        };

        return await _accountRepository.CreateAsync(account, cancellationToken);
    }
}
