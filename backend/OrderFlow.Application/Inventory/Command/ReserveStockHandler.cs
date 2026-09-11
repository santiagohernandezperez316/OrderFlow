using MediatR;
using OrderFlow.Application.Ports;
using OrderFlow.Contracts.Events;
using OrderFlow.Domain.Inventory.Service;

namespace OrderFlow.Application.Inventory.Command;

internal class ReserveStockHandler(
    IProcessedEventStore processedEventStore,
    ReserveStockService reserveStockService,
    IInventoryUnitOfWork unitOfWork,
    IEventPublisher eventPublisher) : IRequestHandler<ReserveStockCommand>
{
    public async Task Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await processedEventStore.HasProcessedAsync(request.EventId);
        if (alreadyProcessed)
        {
            return;
        }

        var reserved = await reserveStockService.ExecuteAsync(request.Sku, request.Cantidad);
        await processedEventStore.MarkProcessedAsync(request.EventId);

        if (reserved)
        {
            await eventPublisher.PublishAsync(new StockReserved(request.OrderId), cancellationToken);
        }
        else
        {
            await eventPublisher.PublishAsync(
                new StockRejected(request.OrderId, $"stock insuficiente para {request.Sku}"),
                cancellationToken);
        }

        await unitOfWork.SaveAsync(cancellationToken);
    }
}
