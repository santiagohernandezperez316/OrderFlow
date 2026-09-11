using NSubstitute;
using OrderFlow.Application.Inventory.Command;
using OrderFlow.Application.Ports;
using OrderFlow.Contracts.Events;
using OrderFlow.Domain.Inventory.Entity;
using OrderFlow.Domain.Inventory.Port;
using OrderFlow.Domain.Inventory.Service;

namespace OrderFlow.Application.Tests.Inventory.Command;

public class ReserveStockHandlerTests
{
    readonly IProcessedEventStore _processedEventStore = Substitute.For<IProcessedEventStore>();
    readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();

    ReserveStockHandler CreateHandler() =>
        new(_processedEventStore, new ReserveStockService(_productRepository), _unitOfWork, _eventPublisher);

    [Fact]
    public async Task Handle_WhenEventAlreadyProcessed_DoesNotTouchStockOrPublish()
    {
        var command = new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU-001", 5);
        _processedEventStore.HasProcessedAsync(command.EventId).Returns(true);

        await CreateHandler().Handle(command, CancellationToken.None);

        await _productRepository.DidNotReceive().GetBySkuAsync(Arg.Any<string>());
        await _processedEventStore.DidNotReceive().MarkProcessedAsync(Arg.Any<Guid>());
        await _eventPublisher.DidNotReceive().PublishAsync(Arg.Any<StockReserved>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishAsync(Arg.Any<StockRejected>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEventNotProcessedAndStockAvailable_ReservesMarksProcessedAndPublishesStockReserved()
    {
        var command = new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU-001", 5);
        _processedEventStore.HasProcessedAsync(command.EventId).Returns(false);
        _productRepository.GetBySkuAsync(command.Sku).Returns(new Product { Id = Guid.NewGuid(), Sku = command.Sku, Name = "Teclado mecanico", Stock = 10 });

        await CreateHandler().Handle(command, CancellationToken.None);

        await _processedEventStore.Received(1).MarkProcessedAsync(command.EventId);
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<StockReserved>(e => e.OrderId == command.OrderId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }
}
