namespace MetaExchange.Domain;

public record ExecutionOrder(
    string ExchangeName,
    OrderType Type,
    decimal BtcAmount,
    decimal PricePerBtc
);