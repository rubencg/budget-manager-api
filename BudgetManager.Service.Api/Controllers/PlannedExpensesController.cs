using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.PlannedExpenses.Commands;
using BudgetManager.Service.Features.PlannedExpenses.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class PlannedExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlannedExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single planned expense by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlannedExpense), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlannedExpense>> GetById(
        string id,
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPlannedExpenseByIdQuery
        {
            PlannedExpenseId = id,
            UserId = userId
        };

        var plannedExpense = await _mediator.Send(query, cancellationToken);

        if (plannedExpense == null)
        {
            return NotFound(new { message = $"Planned expense {id} not found" });
        }

        return Ok(plannedExpense);
    }

    /// <summary>
    /// Create a new planned expense
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlannedExpense), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlannedExpense>> Create(
        [FromBody] CreatePlannedExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        var plannedExpense = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = plannedExpense.Id, userId = plannedExpense.UserId },
            plannedExpense);
    }

    /// <summary>
    /// Update an existing planned expense
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PlannedExpense), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlannedExpense>> Update(
        string id,
        [FromBody] UpdatePlannedExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.PlannedExpenseId)
        {
            return BadRequest(new { message = "Planned expense ID in URL does not match ID in body" });
        }

        try
        {
            var plannedExpense = await _mediator.Send(command, cancellationToken);
            return Ok(plannedExpense);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a planned expense
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePlannedExpenseCommand
        {
            PlannedExpenseId = id,
            UserId = userId
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound(new { message = $"Planned expense {id} not found" });
        }
    }
}
