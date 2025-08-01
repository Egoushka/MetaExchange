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
    public async Task<ActionResult<Features.ExecuteOrder.ExecuteOrderResponse>> ExecuteOrder([FromBody] Features.ExecuteOrder.ExecuteOrderRequest executeOrderRequest)
    {
        var result = await _mediator.Send(executeOrderRequest);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { Errors = result.Errors.Select(e => e.Message) });
    }
}