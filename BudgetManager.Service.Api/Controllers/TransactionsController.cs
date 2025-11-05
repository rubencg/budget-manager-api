using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
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
        [FromQuery] string? accountId = null,
        [FromQuery] string? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsByMonthQuery
        {
            Year = year,
            Month = month,
            TransactionType = type,
            AccountId = accountId,
            CategoryId = categoryId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}