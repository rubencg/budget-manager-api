using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Category>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<UpdateCategoryHandler> _logger;

    public UpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<UpdateCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Category> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating category {CategoryId} for user {UserId}",
            request.CategoryId,
            request.UserId);

        // Get existing category to preserve CreatedAt
        var existingCategory = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            request.UserId,
            cancellationToken);

        if (existingCategory == null)
        {
            throw new InvalidOperationException($"Category {request.CategoryId} not found");
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
