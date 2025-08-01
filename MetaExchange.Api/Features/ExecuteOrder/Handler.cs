using MediatR;
using MetaExchange.Api.Services;
using MetaExchange.Core;

namespace MetaExchange.Api.Features.ExecuteOrder;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly MetaExchangeService _metaExchangeService;
    private readonly IOrderExecutor _orderExecutor;

    public Handler(MetaExchangeService metaExchangeService, IOrderExecutor orderExecutor)
    {
        _metaExchangeService = metaExchangeService;
        _orderExecutor = orderExecutor;
    }

    public Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var exchanges = _metaExchangeService.GetExchanges();

        var executionOrders = _orderExecutor
            .GetBestExecutionPlan(exchanges, request.Type, request.Amount);

        var orderDtos = executionOrders
            .Select(Response.ExecutionOrderDto.FromEntity)
            .ToList();

        if (orderDtos.Count == 0)
        {
            var emptyResponse = new Response([], 0);
            
            return Task.FromResult(emptyResponse);
        }
        
        var averagePrice = orderDtos.Average(o => o.PricePerBtc);

        var response = new Response(orderDtos, averagePrice);

        return Task.FromResult(response);
    }
}