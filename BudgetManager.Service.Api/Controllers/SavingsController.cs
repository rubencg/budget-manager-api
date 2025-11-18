using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.Savings.Commands;
using BudgetManager.Service.Features.Savings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single saving by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Saving), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Saving>> GetById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSavingByIdQuery
        {
            SavingId = id
        };

        var saving = await _mediator.Send(query, cancellationToken);

        if (saving == null)
        {
            return NotFound(new { message = $"Saving {id} not found" });
        }

        return Ok(saving);
    }

    /// <summary>
    /// Create a new saving
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Saving), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Saving>> Create(
        [FromBody] CreateSavingCommand command,
        CancellationToken cancellationToken = default)
    {
        var saving = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = saving.Id },
            saving);
    }

    /// <summary>
    /// Update an existing saving
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Saving), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Saving>> Update(
        string id,
        [FromBody] UpdateSavingCommand command,
        CancellationToken cancellationToken = default)
    {
        var commandWithId = command with { SavingId = id };

        try
        {
            var saving = await _mediator.Send(commandWithId, cancellationToken);
            return Ok(saving);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a saving
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSavingCommand
        {
            SavingId = id
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound(new { message = $"Saving {id} not found" });
        }
    }
}
