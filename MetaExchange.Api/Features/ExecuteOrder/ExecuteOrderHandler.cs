using MediatR;
using MetaExchange.Api.Services;
using MetaExchange.Core;

namespace MetaExchange.Api.Features.ExecuteOrder;

public class ExecuteOrderHandler : IRequestHandler<ExecuteOrderRequest, ExecuteOrderResponse>
{
    private readonly MetaExchangeService _metaExchangeService;
    private readonly IOrderExecutor _orderExecutor;

    public ExecuteOrderHandler(MetaExchangeService metaExchangeService, IOrderExecutor orderExecutor)
    {
        _metaExchangeService = metaExchangeService;
        _orderExecutor = orderExecutor;
    }

    public Task<ExecuteOrderResponse> Handle(ExecuteOrderRequest executeOrderRequest, CancellationToken cancellationToken)
    {
        var exchanges = _metaExchangeService.GetExchanges();

        var executionOrders = _orderExecutor
            .GetBestExecutionPlan(exchanges, executeOrderRequest.Type, executeOrderRequest.Amount);

        var orderDtos = executionOrders
            .Select(ExecuteOrderResponse.ExecutionOrderDto.FromEntity)
            .ToList();

        if (orderDtos.Count == 0)
        {
            var emptyResponse = new ExecuteOrderResponse([], 0);
            
            return Task.FromResult(emptyResponse);
        }
        
        var averagePrice = orderDtos.Average(o => o.PricePerBtc);

        var response = new ExecuteOrderResponse(orderDtos, averagePrice);

        return Task.FromResult(response);
    }
}