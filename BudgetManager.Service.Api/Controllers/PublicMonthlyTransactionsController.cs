using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.MonthlyTransactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/public/monthly-transactions")]
public class PublicMonthlyTransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicMonthlyTransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get overdue monthly expenses for a user by date (unauthenticated)
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(List<MonthlyTransaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MonthlyTransaction>>> GetOverdue(
        [FromQuery] string userId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "userId is required" });

        var query = new GetOverdueMonthlyTransactionsQuery { UserId = userId, Date = date };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
