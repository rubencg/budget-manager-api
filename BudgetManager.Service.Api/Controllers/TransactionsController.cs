using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Features.Transactions.Commands;
using BudgetManager.Service.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> GetById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionByIdQuery
        {
            TransactionId = id
        };

        var transaction = await _mediator.Send(query, cancellationToken);

        if (transaction == null)
        {
            return NotFound(new { message = $"Transaction {id} not found" });
        }

        return Ok(transaction);
    }

    /// <summary>
    /// Get transactions for a specific month
    /// </summary>
    [HttpGet("month/{year}/{month}")]
    [ProducesResponseType(typeof(GetTransactionsByMonthQueryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetTransactionsByMonthQueryResult>> GetByMonth(
        int year,
        int month,
        [FromQuery] TransactionType? type = null,
        [FromQuery] string? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsByMonthQuery
        {
            Year = year,
            Month = month,
            TransactionType = type,
            CategoryId = categoryId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Transaction>> Create(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            transaction);
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Transaction>> Update(
        string id,
        [FromBody] UpdateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.TransactionId)
        {
            return BadRequest(new { message = "Transaction ID in URL does not match ID in body" });
        }

        try
        {
            var transaction = await _mediator.Send(command, cancellationToken);
            return Ok(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand
        {
            TransactionId = id
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound(new { message = $"Transaction {id} not found" });
        }
    }
}