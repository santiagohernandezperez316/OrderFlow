using Microsoft.AspNetCore.SignalR;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Api.Hubs;

public class SignalROrderRealtimeNotifier(IHubContext<OrdersHub> hubContext) : IOrderRealtimeNotifier
{
    public Task NotifyStatusChangedAsync(Guid orderId, OrderStatus status, string? reason, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("OrderStatusChanged", orderId, status, reason, cancellationToken: cancellationToken);
    }
}
