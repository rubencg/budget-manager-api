using BudgetManager.Service.Api.Features.Budget.DTOs;
using BudgetManager.Service.Api.Features.Budget.Queries;
using BudgetManager.Service.Services.UserContext;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/budget")]
public class BudgetController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public BudgetController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("incomeAfterFixedExpenses/{year}/{month}")]
    public async Task<ActionResult<IncomeAfterFixedExpensesDto>> GetIncomeAfterFixedExpenses(int year, int month)
    {
        var result = await _mediator.Send(new GetIncomeAfterFixedExpensesQuery(_currentUserService.UserId, year, month));
        return Ok(result);
    }

    [HttpGet("plannedExpenses/{year}/{month}")]
    public async Task<ActionResult<PlannedExpensesResponseDto>> GetPlannedExpenses(int year, int month, [FromQuery] string? plannedExpenseId)
    {
        var result = await _mediator.Send(new GetPlannedExpensesQuery(_currentUserService.UserId, year, month, plannedExpenseId));
        return Ok(result);
    }

    [HttpGet("otherExpenses/{year}/{month}")]
    public async Task<ActionResult<OtherExpensesResponseDto>> GetOtherExpenses(int year, int month)
    {
        var result = await _mediator.Send(new GetOtherExpensesQuery(_currentUserService.UserId, year, month));
        return Ok(result);
    }
}
