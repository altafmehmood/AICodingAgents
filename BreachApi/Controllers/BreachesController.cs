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

    /// <summary>
    /// Download breaches as PDF, optionally filtered by date range.
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <returns>PDF file</returns>
    [HttpGet("pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetPdf([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = new GetBreachesPdfQuery(from, to);
        var pdfBytes = await _mediator.Send(query);

        // Generate filename
        var filename = "breaches";
        if (from.HasValue && to.HasValue)
        {
            filename += $"-{from.Value:yyyyMMdd}-{to.Value:yyyyMMdd}";
        }
        else if (from.HasValue)
        {
            filename += $"-from-{from.Value:yyyyMMdd}";
        }
        else if (to.HasValue)
        {
            filename += $"-to-{to.Value:yyyyMMdd}";
        }
        else
        {
            filename += $"-{DateTime.Now:yyyyMMdd}";
        }
        filename += ".pdf";

        return File(pdfBytes, "application/pdf", filename);
    }
} 