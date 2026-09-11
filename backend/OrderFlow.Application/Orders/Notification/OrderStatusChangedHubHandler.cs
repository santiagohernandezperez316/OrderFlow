using MediatR;
using OrderFlow.Application.Ports;

namespace OrderFlow.Application.Orders.Notification;

internal class OrderStatusChangedHubHandler(IOrderRealtimeNotifier notifier) : INotificationHandler<OrderStatusChangedNotification>
{
    public Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        return notifier.NotifyStatusChangedAsync(notification.OrderId, notification.Status, notification.Reason, cancellationToken);
    }
}
