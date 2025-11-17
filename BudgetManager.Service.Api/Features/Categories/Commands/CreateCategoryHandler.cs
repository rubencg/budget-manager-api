using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Category>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateCategoryHandler> _logger;

    public CreateCategoryHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating category {CategoryName} for user {UserId}",
            request.Name,
            userId);

        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = request.Name,
            Image = request.Image,
            Color = request.Color,
            CategoryType = request.CategoryType,
            Subcategories = request.Subcategories
        };

        return await _categoryRepository.CreateAsync(category, cancellationToken);
    }
}
