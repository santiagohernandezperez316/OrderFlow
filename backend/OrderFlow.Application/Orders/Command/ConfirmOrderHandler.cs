using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Orders.Notification;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Service;

namespace OrderFlow.Application.Orders.Command;

internal class ConfirmOrderHandler(
    ConfirmOrderService confirmOrderService,
    IOrdersUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<ConfirmOrderHandler> logger) : IRequestHandler<ConfirmOrderCommand>
{
    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await confirmOrderService.ExecuteAsync(request.OrderId);
        await unitOfWork.SaveAsync(cancellationToken);

        if (result.Transitioned)
        {
            await publisher.Publish(new OrderStatusChangedNotification(request.OrderId, OrderStatus.Confirmed), cancellationToken);
        }
        else
        {
            logger.LogWarning(
                "Se ignoro Confirm para la orden {OrderId} (evento {EventId}): el pedido no esta en Pending (estado actual: {CurrentStatus}).",
                request.OrderId, request.EventId, result.CurrentStatus);
        }
    }
}
