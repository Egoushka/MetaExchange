using MetaExchange.Domain;

namespace MetaExchange.Console;

public record ExecutionInput(OrderType Type, decimal Amount);