namespace MetaExchange.Domain;

public record OrderBook(
    List<OrderBookEntry> Asks,
    List<OrderBookEntry> Bids
);