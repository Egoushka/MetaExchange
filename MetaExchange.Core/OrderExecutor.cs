using FluentResults;
using MetaExchange.Domain;

namespace MetaExchange.Core;

using System.Collections.Generic;
using System.Linq;

public class OrderExecutor : IOrderExecutor
{
    private const decimal Epsilon = 0.00000001m;

    public Result<List<ExecutionOrder>> GetBestExecutionPlan(List<Exchange> exchanges, OrderType type, decimal targetBtcAmount)
    {
        if (targetBtcAmount <= Epsilon)
        {
            return Result.Ok(new List<ExecutionOrder>());
        }
        
        return type == OrderType.Buy
            ? ExecuteBuyPlan(exchanges, targetBtcAmount)
            : ExecuteSellPlan(exchanges, targetBtcAmount);
    }

    private Result<List<ExecutionOrder>> ExecuteBuyPlan(List<Exchange> exchanges, decimal targetBtcAmount)
    {
        var plan = new List<ExecutionOrder>();
        var remainingBtcToProcess = targetBtcAmount;
        
        var exchangesByName = exchanges.ToDictionary(ex => ex.Name);
        var exchangeBalances = exchanges.ToDictionary(ex => ex.Name, ex => ex.EurBalance);
        var nextAskIndex = exchanges.ToDictionary(ex => ex.Name, _ => 0);

        var queue = new PriorityQueue<(string ExchangeName, decimal Price), decimal>();
        foreach (var ex in exchanges)
        {
            if (ex.Book.Asks.Any())
            {
                queue.Enqueue((ex.Name, ex.Book.Asks[0].Price), ex.Book.Asks[0].Price);
            }
        }

        while (remainingBtcToProcess > Epsilon && queue.Count > 0)
        {
            var (exchangeName, price) = queue.Dequeue();
            
            var exchange = exchangesByName[exchangeName];
            var askIndex = nextAskIndex[exchangeName];
            
            var offer = exchange.Book.Asks[askIndex];

            decimal eurBalance = exchangeBalances[exchangeName];
            
            decimal maxAmountDueToBalance = (price > 0) ? eurBalance / price : 0;
            decimal executableAmount = Math.Min(offer.Amount, maxAmountDueToBalance);
            decimal amountToTake = Math.Min(remainingBtcToProcess, executableAmount);

            if (amountToTake > Epsilon)
            {
                plan.Add(new ExecutionOrder(exchangeName, OrderType.Buy, amountToTake, price));
                remainingBtcToProcess -= amountToTake;
                exchangeBalances[exchangeName] -= amountToTake * price;
            }

            nextAskIndex[exchangeName]++;
            if (nextAskIndex[exchangeName] < exchange.Book.Asks.Count)
            {
                var nextOffer = exchange.Book.Asks[nextAskIndex[exchangeName]];
                queue.Enqueue((exchangeName, nextOffer.Price), nextOffer.Price);
            }
        }

        if (remainingBtcToProcess > Epsilon)
            return Result.Fail("Not enough liquidity or balance to fulfill the buy order.");

        return Result.Ok(plan);
    }
    
    private Result<List<ExecutionOrder>> ExecuteSellPlan(List<Exchange> exchanges, decimal targetBtcAmount)
    {
        var plan = new List<ExecutionOrder>();
        var remainingBtcToProcess = targetBtcAmount;
        
        var exchangesByName = exchanges.ToDictionary(ex => ex.Name);
        var exchangeBalances = exchanges.ToDictionary(ex => ex.Name, ex => ex.BtcBalance);
        var nextBidIndex = exchanges.ToDictionary(ex => ex.Name, _ => 0);
        
        var queue = new PriorityQueue<(string ExchangeName, decimal Price), decimal>();
        foreach (var ex in exchanges)
        {
            if (ex.Book.Bids.Any())
            {
                var price = ex.Book.Bids[0].Price;
                queue.Enqueue((ex.Name, price), -price);
            }
        }

        while (remainingBtcToProcess > Epsilon && queue.Count > 0)
        {
            var (exchangeName, price) = queue.Dequeue();
            var ex = exchangesByName[exchangeName];
            var bidIndex = nextBidIndex[exchangeName];
            var offer = ex.Book.Bids[bidIndex];

            decimal btcBalance = exchangeBalances[exchangeName];
            decimal executableAmount = Math.Min(offer.Amount, btcBalance);
            decimal amountToTake = Math.Min(remainingBtcToProcess, executableAmount);

            if (amountToTake > Epsilon)
            {
                plan.Add(new ExecutionOrder(exchangeName, OrderType.Sell, amountToTake, price));
                remainingBtcToProcess -= amountToTake;
                exchangeBalances[exchangeName] -= amountToTake;
            }
            
            nextBidIndex[exchangeName]++;
            if (nextBidIndex[exchangeName] < ex.Book.Bids.Count)
            {
                var nextOffer = ex.Book.Bids[nextBidIndex[exchangeName]];
                queue.Enqueue((exchangeName, nextOffer.Price), -nextOffer.Price); 
            }
        }

        if (remainingBtcToProcess > Epsilon)
            return Result.Fail("Not enough liquidity or balance to fulfill the sell order.");

        return Result.Ok(plan);
    }
}