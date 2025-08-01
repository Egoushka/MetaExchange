using MetaExchange.Core;
using MetaExchange.Api.Dtos;
using MetaExchange.Api.Requests;
using MetaExchange.Api.Services;
using MetaExchange.Domain;
using Microsoft.AspNetCore.Mvc;

namespace MetaExchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaExchangeController : ControllerBase
{
    private readonly MetaExchangeService _metaExchangeService;
    private readonly IOrderExecutor _orderExecutor;

    public MetaExchangeController(MetaExchangeService metaExchangeService, IOrderExecutor orderExecutor)
    {
        _metaExchangeService = metaExchangeService;
        _orderExecutor = orderExecutor;
    }
    
    [HttpPost("execute")]
    public ActionResult<ExecuteOrderDto> ExecuteOrder([FromBody] ExecuteOrderRequest request)
    {
        var exchanges = _metaExchangeService.GetExchanges(); 
        try
        {
            var executionOrders = _orderExecutor
                .GetBestExecutionPlan(exchanges, request.Type, request.Amount)
                .Select(ExecuteOrderDto.ExecutionOrderDto.FromEntity)
                .ToList();
            
            var averagePrice = executionOrders.Average(o => o.PricePerBtc);

            var result = new ExecuteOrderDto(executionOrders, averagePrice);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}