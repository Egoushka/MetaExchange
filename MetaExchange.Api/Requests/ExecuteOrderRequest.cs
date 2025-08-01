using MetaExchange.Domain;

namespace MetaExchange.Api.Requests;

public record ExecuteOrderRequest(OrderType Type, decimal Amount);