using MassTransit;
using MediatR;
using OrderFlow.Application.Orders.Command;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Consumers;

public class StockRejectedConsumer(IMediator mediator) : IConsumer<StockRejected>
{
    public async Task Consume(ConsumeContext<StockRejected> context) =>
        await mediator.Send(new RejectOrderCommand(context.Message.OrderId, context.Message.Reason, context.MessageId ?? Guid.NewGuid()));
}
