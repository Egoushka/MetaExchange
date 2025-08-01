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
        try
        {
            var result = await _mediator.Send(executeOrderRequest);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}