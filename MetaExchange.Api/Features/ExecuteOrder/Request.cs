using MediatR;
using MetaExchange.Domain;

namespace MetaExchange.Api.Features.ExecuteOrder;

public record Request(OrderType Type, decimal Amount) : IRequest<Response>;