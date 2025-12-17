using BudgetManager.Service.Api.Features.Budget.DTOs;
using MediatR;

namespace BudgetManager.Service.Api.Features.Budget.Queries;

public record GetPlannedExpensesQuery(string UserId, int Year, int Month, string? PlannedExpenseId) : IRequest<PlannedExpensesResponseDto>;
