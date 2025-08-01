using MetaExchange.Domain;

namespace MetaExchange.Core;

public interface IOrderExecutor
{
    List<ExecutionOrder> GetBestExecutionPlan(
        List<Exchange> exchanges,
        OrderType type,
        decimal targetBtcAmount);
}