using FluentResults;
using MetaExchange.Domain;

namespace MetaExchange.Core;

public class OrderExecutor : IOrderExecutor
{
    public Result<List<ExecutionOrder>> GetBestExecutionPlan(List<Exchange> exchanges, OrderType type, decimal targetBtcAmount)
    {
        return type == OrderType.Buy
            ? ExecutePlan(
                exchanges,
                targetBtcAmount,
                ex => new ExchangeState(ex.EurBalance, ex.Book.Asks),
                (offer) => offer.Price,
                (state, btcAmount, price) => state.Balance -= btcAmount * price,
                (state) => state.Balance / state.GetCurrentOffer().Price,
                OrderType.Buy
            )
            : ExecutePlan(
                exchanges,
                targetBtcAmount,
                ex => new ExchangeState(ex.BtcBalance, ex.Book.Bids),
                (offer) => -offer.Price,
                (state, btcAmount, price) => state.Balance -= btcAmount,
                (state) => state.Balance,
                OrderType.Sell
            );
    }

    private Result<List<ExecutionOrder>> ExecutePlan(
        List<Exchange> exchanges,
        decimal targetBtcAmount,
        Func<Exchange, ExchangeState> stateSelector,
        Func<OrderBookEntry, decimal> prioritySelector,
        Action<ExchangeState, decimal, decimal> updateBalance,
        Func<ExchangeState, decimal> maxAmountSelector,
        OrderType orderType
    )
    {
        var plan = new List<ExecutionOrder>();
        var remainingBtc = targetBtcAmount;

        var states = exchanges.Select(ex => (ex.Name, stateSelector(ex))).ToList();

        var queue = PrepareQueue(states, prioritySelector);

        while (remainingBtc > Constants.Satoshi && queue.Count > 0)
        {
            var (exchangeName, currentOffer) = queue.Dequeue();
            var state = states.First(s => s.Name == exchangeName).Item2;

            decimal maxAmountFromBalance = maxAmountSelector(state);
            decimal executableAmount = Math.Min(currentOffer.Amount, maxAmountFromBalance);
            decimal amountToTake = Math.Min(remainingBtc, executableAmount);

            if (amountToTake > Constants.Satoshi)
            {
                plan.Add(new ExecutionOrder(exchangeName, orderType, amountToTake, currentOffer.Price));
                remainingBtc -= amountToTake;
                updateBalance(state, amountToTake, currentOffer.Price);
            }

            state.MoveToNextOffer();
            if (state.HasMoreOffers())
            {
                var nextOffer = state.GetCurrentOffer();
                queue.Enqueue((exchangeName, nextOffer), prioritySelector(nextOffer));
            }
        }

        if (remainingBtc > Constants.Satoshi)
            return Result.Fail(orderType == OrderType.Buy
                ? "Not enough liquidity or EUR balance to fulfill the buy order."
                : "Not enough liquidity or BTC balance to fulfill the sell order.");

        return Result.Ok(plan);
    }

    private PriorityQueue<(string ExchangeName, OrderBookEntry Offer), decimal> PrepareQueue(
        List<(string Name, ExchangeState State)> states,
        Func<OrderBookEntry, decimal> prioritySelector)
    {
        var queue = new PriorityQueue<(string ExchangeName, OrderBookEntry Offer), decimal>();
        foreach (var (name, state) in states)
        {
            if (state.HasMoreOffers())
            {
                var offer = state.GetCurrentOffer();
                queue.Enqueue((name, offer), prioritySelector(offer));
            }
        }
        return queue;
    }
}