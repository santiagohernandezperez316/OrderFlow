using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Orders.Notification;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Service;

namespace OrderFlow.Application.Orders.Command;

internal class RejectOrderHandler(
    RejectOrderService rejectOrderService,
    IOrdersUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<RejectOrderHandler> logger) : IRequestHandler<RejectOrderCommand>
{
    public async Task Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await rejectOrderService.ExecuteAsync(request.OrderId, request.Reason);
        await unitOfWork.SaveAsync(cancellationToken);

        if (result.Transitioned)
        {
            await publisher.Publish(
                new OrderStatusChangedNotification(request.OrderId, OrderStatus.Rejected, request.Reason),
                cancellationToken);
        }
        else
        {
            logger.LogWarning(
                "Se ignoro Reject para la orden {OrderId} (evento {EventId}): el pedido no esta en Pending (estado actual: {CurrentStatus}).",
                request.OrderId, request.EventId, result.CurrentStatus);
        }
    }
}
