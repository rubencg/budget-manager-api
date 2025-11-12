using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<DeleteCategoryHandler> _logger;

    public DeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<DeleteCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting category {CategoryId} for user {UserId}",
            request.CategoryId,
            request.UserId);

        await _categoryRepository.DeleteAsync(
            request.CategoryId,
            request.UserId,
            cancellationToken);
    }
}
