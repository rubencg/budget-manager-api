using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteCategoryHandler> _logger;

    public DeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting category {CategoryId} for user {UserId}",
            request.CategoryId,
            userId);

        await _categoryRepository.DeleteAsync(
            request.CategoryId,
            userId,
            cancellationToken);
    }
}
