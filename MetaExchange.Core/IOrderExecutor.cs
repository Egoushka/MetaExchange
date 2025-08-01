using FluentResults;
using MetaExchange.Domain;

namespace MetaExchange.Core;

public interface IOrderExecutor
{
    Result<List<ExecutionOrder>> GetBestExecutionPlan(
        List<Exchange> exchanges,
        OrderType type,
        decimal targetBtcAmount);
}