using MassTransit;
using MediatR;
using OrderFlow.Application.Inventory.Command;
using OrderFlow.Contracts.Events;

namespace OrderFlow.InventoryWorker.Consumers;

public class OrderCreatedConsumer(IMediator mediator) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;
        var eventId = context.MessageId ?? Guid.NewGuid();

        await mediator.Send(new ReserveStockCommand(eventId, message.OrderId, message.Sku, message.Cantidad));
    }
}
