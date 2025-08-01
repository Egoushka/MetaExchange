using FluentResults;
using MediatR;
using MetaExchange.Domain;

namespace MetaExchange.Api.Features.ExecuteOrder;

public record ExecuteOrderRequest(OrderType Type, decimal Amount) : IRequest<Result<ExecuteOrderResponse>>;