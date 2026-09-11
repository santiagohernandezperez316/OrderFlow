using OrderFlow.Domain.Common;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Port;

namespace OrderFlow.Domain.Orders.Service;

[DomainService]
public class CreateOrderService(IOrderRepository orderRepository)
{
    public async Task ExecuteAsync(Order order)
    {
        await orderRepository.AddAsync(order);
    }
}
