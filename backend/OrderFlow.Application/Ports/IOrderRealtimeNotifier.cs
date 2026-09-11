using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Application.Ports;

public interface IOrderRealtimeNotifier
{
    Task NotifyStatusChangedAsync(Guid orderId, OrderStatus status, string? reason, CancellationToken cancellationToken);
}
