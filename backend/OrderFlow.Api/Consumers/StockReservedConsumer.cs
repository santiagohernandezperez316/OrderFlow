using MassTransit;
using MediatR;
using OrderFlow.Application.Orders.Command;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Consumers;

public class StockReservedConsumer(IMediator mediator) : IConsumer<StockReserved>
{
    public async Task Consume(ConsumeContext<StockReserved> context) =>
        await mediator.Send(new ConfirmOrderCommand(context.Message.OrderId, context.MessageId ?? Guid.NewGuid()));
}
