using BudgetManager.Service.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard data including balance, recent transactions, and calendar view
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetDashboardQueryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetDashboardQueryResult>> GetDashboard(
        [FromQuery] string userId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            UserId = userId,
            Year = year,
            Month = month
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
