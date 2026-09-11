using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Application.Orders.Notification;

internal class OrderStatusChangedLogHandler(ILogger<OrderStatusChangedLogHandler> logger) : INotificationHandler<OrderStatusChangedNotification>
{
    public Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order {OrderId} changed status to {Status}", notification.OrderId, notification.Status);
        return Task.CompletedTask;
    }
}
