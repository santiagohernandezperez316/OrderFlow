using OrderFlow.Domain.Common;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Port;

namespace OrderFlow.Domain.Orders.Service;

[DomainService]
public class RejectOrderService(IOrderRepository orderRepository)
{
    public async Task<OrderTransitionResult> ExecuteAsync(Guid orderId, string? reason = null)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order is null)
        {
            return new OrderTransitionResult(false, null);
        }

        var transitioned = order.Reject(reason);
        if (transitioned)
        {
            orderRepository.Update(order);
        }

        return new OrderTransitionResult(transitioned, order.Status);
    }
}
