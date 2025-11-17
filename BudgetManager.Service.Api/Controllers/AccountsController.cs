using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.Accounts.Commands;
using BudgetManager.Service.Features.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single account by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> GetById(
        string id,
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccountByIdQuery
        {
            AccountId = id,
            UserId = userId
        };

        var account = await _mediator.Send(query, cancellationToken);

        if (account == null)
        {
            return NotFound(new { message = $"Account {id} not found" });
        }

        return Ok(account);
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Account), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Account>> Create(
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var account = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id, userId = account.UserId },
            account);
    }

    /// <summary>
    /// Update an existing account
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Account>> Update(
        string id,
        [FromBody] UpdateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.AccountId)
        {
            return BadRequest(new { message = "Account ID in URL does not match ID in body" });
        }

        try
        {
            var account = await _mediator.Send(command, cancellationToken);
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteAccountCommand
        {
            AccountId = id,
            UserId = userId
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound(new { message = $"Account {id} not found" });
        }
    }
}
