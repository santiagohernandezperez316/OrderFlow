using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Application.Orders.Command.Factory;

public class OrderFactory
{
    public Order Create(CreateOrderCommand command)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            ClienteNombre = command.ClienteNombre,
            Sku = command.Sku,
            Cantidad = command.Cantidad
        };
    }
}
