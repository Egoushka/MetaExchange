using MetaExchange.Domain;

namespace MetaExchange.Api.Dtos;

public record ExecuteOrderDto(List<ExecuteOrderDto.ExecutionOrderDto> Orders, decimal AveragePrice)
{
    public record ExecutionOrderDto(string ExchangeName, OrderType Type, decimal BtcAmount, decimal PricePerBtc)
    {
        public static ExecutionOrderDto FromEntity(ExecutionOrder order)
        {
            return new ExecutionOrderDto(order.ExchangeName, order.Type, order.BtcAmount, order.PricePerBtc);
        }
    }
}


