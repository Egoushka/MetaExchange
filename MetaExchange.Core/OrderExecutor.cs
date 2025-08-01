namespace MetaExchange.Core;

using MetaExchange.Domain;
using System.Collections.Generic;
using System.Linq;

public class OrderExecutor : IOrderExecutor
{
    public List<ExecutionOrder> GetBestExecutionPlan(List<Exchange> exchanges, OrderType type, decimal targetBtcAmount)
    {
        return type == OrderType.Buy
            ? ExecuteBuyPlan(exchanges, targetBtcAmount)
            : ExecuteSellPlan(exchanges, targetBtcAmount);
    }

    private List<ExecutionOrder> ExecuteBuyPlan(List<Exchange> exchanges, decimal targetBtcAmount)
    {
        var plan = new List<ExecutionOrder>();
        var remainingBtcToProcess = targetBtcAmount;
        
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

        while (remainingBtcToProcess > 0.00000001m && queue.Count > 0)
        {
            // Dequeue the globally best offer (lowest price).
            var (exchangeName, price) = queue.Dequeue();
            
            var exchange = exchanges.First(e => e.Name == exchangeName);
            var askIndex = nextAskIndex[exchangeName];
            
            var offer = exchange.Book.Asks[askIndex];

            decimal eurBalance = exchangeBalances[exchangeName];
            decimal maxAmountDueToBalance = (price > 0) ? eurBalance / price : 0;
            decimal executableAmount = Math.Min(offer.Amount, maxAmountDueToBalance);
            decimal amountToTake = Math.Min(remainingBtcToProcess, executableAmount);

            if (amountToTake > 0)
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

        if (remainingBtcToProcess > 0.00000001m)
            throw new InvalidOperationException("Not enough liquidity or balance to fulfill the buy order.");

        return plan;
    }
    
    private List<ExecutionOrder> ExecuteSellPlan(List<Exchange> exchanges, decimal targetBtcAmount)
    {
        var plan = new List<ExecutionOrder>();
        var remainingBtcToProcess = targetBtcAmount;

        var exchangeBalances = exchanges.ToDictionary(ex => ex.Name, ex => ex.BtcBalance);
        var nextBidIndex = exchanges.ToDictionary(ex => ex.Name, _ => 0);
        
        // To find the highest price, we use a Min-Heap but give it the NEGATIVE price as priority.
        // The smallest negative number corresponds to the largest positive number.
        var queue = new PriorityQueue<(string ExchangeName, decimal Price), decimal>();
        
        foreach (var ex in exchanges)
        {
            if (ex.Book.Bids.Any())
            {
                var price = ex.Book.Bids[0].Price;
                queue.Enqueue((ex.Name, price), -price); // Note the negative priority
            }
        }

        while (remainingBtcToProcess > 0.00000001m && queue.Count > 0)
        {
            var (exchangeName, price) = queue.Dequeue();
            var ex = exchanges.First(e => e.Name == exchangeName);
            var bidIndex = nextBidIndex[exchangeName];
            var offer = ex.Book.Bids[bidIndex];

            decimal btcBalance = exchangeBalances[exchangeName];
            decimal executableAmount = Math.Min(offer.Amount, btcBalance);
            decimal amountToTake = Math.Min(remainingBtcToProcess, executableAmount);

            if (amountToTake > 0)
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

        if (remainingBtcToProcess > 0.00000001m)
            throw new InvalidOperationException("Not enough liquidity or balance to fulfill the sell order.");

        return plan;
    }
}