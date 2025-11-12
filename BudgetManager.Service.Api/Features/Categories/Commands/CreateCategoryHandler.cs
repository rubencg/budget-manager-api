using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Category>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CreateCategoryHandler> _logger;

    public CreateCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<CreateCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating category {CategoryName} for user {UserId}",
            request.Name,
            request.UserId);

        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            Name = request.Name,
            Image = request.Image,
            Color = request.Color,
            CategoryType = request.CategoryType,
            Subcategories = request.Subcategories
        };

        return await _categoryRepository.CreateAsync(category, cancellationToken);
    }
}
