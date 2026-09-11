using MediatR;
using OrderFlow.Application.Orders.Query.Dto;

namespace OrderFlow.Application.Orders.Command;

public record CreateOrderCommand(string ClienteNombre, string Sku, int Cantidad) : IRequest<OrderDto>;
