using MetaExchange.Domain;

namespace MetaExchange.Api.Features.ExecuteOrder;

public record ExecuteOrderResponse(List<ExecuteOrderResponse.ExecutionOrderDto> Orders, decimal AveragePrice)
{
    public record ExecutionOrderDto(string ExchangeName, OrderType Type, decimal BtcAmount, decimal PricePerBtc)
    {
        public static ExecutionOrderDto FromEntity(ExecutionOrder order)
        {
            return new ExecutionOrderDto(order.ExchangeName, order.Type, order.BtcAmount, order.PricePerBtc);
        }
    }
}