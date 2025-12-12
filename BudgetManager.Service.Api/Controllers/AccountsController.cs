using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.Accounts.Commands;
using BudgetManager.Service.Features.Accounts.Queries;
using BudgetManager.Service.Infrastructure.Pagination;
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
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccountByIdQuery
        {
            AccountId = id
        };

        var account = await _mediator.Send(query, cancellationToken);

        if (account == null)
        {
            return NotFound(new { message = $"Account {id} not found" });
        }

        return Ok(account);
    }

    /// <summary>
    /// Get accounts for dashboard grouped by type
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(List<AccountDashboardGroupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccountDashboardGroupDto>>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccountsDashboardQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get archived accounts with pagination and sorting
    /// </summary>
    [HttpGet("archived")]
    [ProducesResponseType(typeof(PagedResult<Account>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Account>>> GetArchived(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "updatedAt",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetArchivedAccountsQuery
        {
            Pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            Sort = new SortParams
            {
                SortBy = sortBy,
                SortDirection = sortDirection
            }
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
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
            new { id = account.Id },
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
        var commandWithId = command with { AccountId = id };

        try
        {
            var account = await _mediator.Send(commandWithId, cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteAccountCommand
        {
            AccountId = id
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
