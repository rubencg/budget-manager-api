using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public class CreateSavingHandler : IRequestHandler<CreateSavingCommand, Saving>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateSavingHandler> _logger;

    public CreateSavingHandler(
        ISavingRepository savingRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateSavingHandler> logger)
    {
        _savingRepository = savingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Saving> Handle(CreateSavingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating saving {SavingName} for user {UserId}",
            request.Name,
            userId);

        var saving = new Saving
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ItemType = "saving",
            Name = request.Name,
            Icon = request.Icon,
            Color = request.Color,
            GoalAmount = request.GoalAmount,
            SavedAmount = request.SavedAmount,
            AmountPerMonth = request.AmountPerMonth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _savingRepository.CreateAsync(saving, cancellationToken);
    }
}
