using BudgetManager.Service.Api.Features.Budget.DTOs;
using MediatR;

namespace BudgetManager.Service.Api.Features.Budget.Queries;

public record GetIncomeAfterFixedExpensesQuery(string UserId, int Year, int Month) : IRequest<IncomeAfterFixedExpensesDto>;
