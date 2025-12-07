using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Features.Categories.Commands;
using BudgetManager.Service.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManager.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all expense categories
    /// </summary>
    [HttpGet("expense")]
    [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Category>>> GetExpenseCategories(
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesByTypeQuery
        {
            CategoryType = BudgetManager.Api.Domain.Enums.CategoryType.Expense
        };

        var categories = await _mediator.Send(query, cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Get all income categories
    /// </summary>
    [HttpGet("income")]
    [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Category>>> GetIncomeCategories(
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesByTypeQuery
        {
            CategoryType = BudgetManager.Api.Domain.Enums.CategoryType.Income
        };

        var categories = await _mediator.Send(query, cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Get a single category by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery
        {
            CategoryId = id
        };

        var category = await _mediator.Send(query, cancellationToken);

        if (category == null)
        {
            return NotFound(new { message = $"Category {id} not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Category>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Category>> Update(
        string id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var commandWithId = command with { CategoryId = id };

        try
        {
            var category = await _mediator.Send(commandWithId, cancellationToken);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand
        {
            CategoryId = id
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound(new { message = $"Category {id} not found" });
        }
    }
}
