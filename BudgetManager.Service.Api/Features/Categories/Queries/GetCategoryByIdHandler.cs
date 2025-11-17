using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Queries;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, Category?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCategoryByIdHandler> _logger;

    public GetCategoryByIdHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ILogger<GetCategoryByIdHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting category {CategoryId} for user {UserId}",
            request.CategoryId,
            userId);

        return await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            userId,
            cancellationToken);
    }
}
