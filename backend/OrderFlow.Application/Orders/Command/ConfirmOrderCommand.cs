using MediatR;

namespace OrderFlow.Application.Orders.Command;

public record ConfirmOrderCommand(Guid OrderId, Guid EventId) : IRequest;
