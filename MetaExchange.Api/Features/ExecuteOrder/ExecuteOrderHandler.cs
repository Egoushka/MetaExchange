using FluentResults;
using MediatR;
using MetaExchange.Api.Services;
using MetaExchange.Core;

namespace MetaExchange.Api.Features.ExecuteOrder;

public class ExecuteOrderHandler : IRequestHandler<ExecuteOrderRequest, Result<ExecuteOrderResponse>>
{
    private readonly MetaExchangeService _metaExchangeService;
    private readonly IOrderExecutor _orderExecutor;

    public ExecuteOrderHandler(MetaExchangeService metaExchangeService, IOrderExecutor orderExecutor)
    {
        _metaExchangeService = metaExchangeService;
        _orderExecutor = orderExecutor;
    }

    public Task<Result<ExecuteOrderResponse>> Handle(ExecuteOrderRequest executeOrderRequest, CancellationToken cancellationToken)
    {
        if (executeOrderRequest.Amount < Constants.Satoshi)
        {
            return Task.FromResult(Result.Fail<ExecuteOrderResponse>(
                new Error("Order amount is too small. It must be at least 0.00000001 BTC.")));
        }
        
        var exchanges = _metaExchangeService.GetExchanges();

        if (exchanges.Count == 0)
        {
            return Task.FromResult(Result.Fail<ExecuteOrderResponse>(
                new Error("No exchange data is available to process the order.")));
        }

        var executionPlanResult = _orderExecutor
            .GetBestExecutionPlan(exchanges, executeOrderRequest.Type, executeOrderRequest.Amount);

        if (executionPlanResult.IsFailed)
        {
            return Task.FromResult(Result.Fail<ExecuteOrderResponse>(executionPlanResult.Errors));
        }

        var executionOrders = executionPlanResult.Value;
        var orderDtos = executionOrders
            .Select(ExecuteOrderResponse.ExecutionOrderDto.FromEntity)
            .ToList();

        if (orderDtos.Count == 0)
        {
            var emptyResponse = new ExecuteOrderResponse([], 0);
            
            return Task.FromResult(Result.Ok(emptyResponse));
        }
        
        var averagePrice = orderDtos.Average(o => o.PricePerBtc);
        var response = new ExecuteOrderResponse(orderDtos, averagePrice);

        return Task.FromResult(Result.Ok(response));
    }
}