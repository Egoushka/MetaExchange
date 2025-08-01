using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MetaExchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaExchangeController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetaExchangeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<Features.ExecuteOrder.Response>> ExecuteOrder([FromBody] Features.ExecuteOrder.Request request)
    {
        try
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}