using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Category>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateCategoryHandler> _logger;

    public UpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Category> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Updating category {CategoryId} for user {UserId}",
            request.CategoryId,
            userId);

        // Get existing category to preserve CreatedAt
        var existingCategory = await _categoryRepository.GetByIdAsync(
            request.CategoryId!,
            userId,
            cancellationToken);

        if (existingCategory == null)
        {
            throw new InvalidOperationException($"Category {request.CategoryId!} not found");
        }

        // Update category with new values
        existingCategory.Name = request.Name;
        existingCategory.Image = request.Image;
        existingCategory.Color = request.Color;
        existingCategory.CategoryType = request.CategoryType;
        existingCategory.Subcategories = request.Subcategories;

        return await _categoryRepository.UpdateAsync(existingCategory, cancellationToken);
    }
}
