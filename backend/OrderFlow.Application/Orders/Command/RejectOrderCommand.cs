using MediatR;

namespace OrderFlow.Application.Orders.Command;

public record RejectOrderCommand(Guid OrderId, string Reason, Guid EventId) : IRequest;
