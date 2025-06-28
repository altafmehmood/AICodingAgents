using MediatR;
using Microsoft.AspNetCore.Mvc;
using BreachApi.Features.Breaches.Queries;
using BreachApi.Models;

namespace BreachApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreachesController : ControllerBase
{
    private readonly IMediator _mediator;
    public BreachesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all breaches, optionally filtered by date range.
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <returns>List of breaches</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Breach>), 200)]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = new GetBreachesQuery(from, to);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
} 