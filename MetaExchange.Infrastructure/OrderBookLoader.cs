using System.Text.Json;
using MetaExchange.Domain;

namespace MetaExchange.Infrastructure;

public static class OrderBookLoader
{
    public static List<Exchange> Load(string filePath)
    {
        var result = new List<Exchange>();
        var lines = File.ReadAllLines(filePath);

        var index = 1;
        
        foreach (var line in lines)
        {
            var tabIndex = line.IndexOf('\t');
            if (tabIndex < 0) continue;

            var jsonPart = line.Substring(tabIndex + 1);

            var rawBook = JsonSerializer.Deserialize<RawOrderBook>(jsonPart);

            if (rawBook is null)
                continue;

            var bids = rawBook.Bids
                .Select(b => new OrderBookEntry(b.Order.Price, b.Order.Amount))
                .ToList();

            var asks = rawBook.Asks
                .Select(a => new OrderBookEntry(a.Order.Price, a.Order.Amount))
                .ToList();

            var book = new OrderBook(asks, bids);

            var exchange = new Exchange(
                Name: $"Exchange{index++}",
                Book: book,
                EurBalance: 100_000m,
                BtcBalance: 100m
            );

            result.Add(exchange);
        }

        return result;
    }
}