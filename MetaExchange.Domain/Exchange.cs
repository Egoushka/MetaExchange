namespace MetaExchange.Domain;

public record Exchange(
    string Name,
    OrderBook Book,
    decimal EurBalance,
    decimal BtcBalance
);