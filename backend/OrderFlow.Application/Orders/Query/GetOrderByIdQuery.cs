using MediatR;
using OrderFlow.Application.Orders.Query.Dto;

namespace OrderFlow.Application.Orders.Query;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;
