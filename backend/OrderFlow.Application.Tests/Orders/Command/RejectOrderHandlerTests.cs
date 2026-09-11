using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.Application.Orders.Command;
using OrderFlow.Application.Orders.Notification;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Port;
using OrderFlow.Domain.Orders.Service;

namespace OrderFlow.Application.Tests.Orders.Command;

public class RejectOrderHandlerTests
{
    readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    readonly IOrdersUnitOfWork _unitOfWork = Substitute.For<IOrdersUnitOfWork>();
    readonly IPublisher _publisher = Substitute.For<IPublisher>();
    readonly RecordingLogger<RejectOrderHandler> _logger = new();

    RejectOrderHandler CreateHandler() =>
        new(new RejectOrderService(_orderRepository), _unitOfWork, _publisher, _logger);

    [Fact]
    public async Task Handle_WhenOrderIsPending_TransitionsToRejected_PersistsReason_AndPublishesNotificationWithReason()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClienteNombre = "Juan Perez",
            Sku = "SKU-001",
            Cantidad = 1
        };

        _orderRepository.GetByIdAsync(order.Id).Returns(order);

        var eventId = Guid.NewGuid();
        var reason = "stock insuficiente para SKU-001";
        var command = new RejectOrderCommand(order.Id, reason, eventId);

        await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(OrderStatus.Rejected, order.Status);
        Assert.Equal(reason, order.RejectionReason);
        _orderRepository.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<OrderStatusChangedNotification>(n =>
                n.OrderId == order.Id && n.Status == OrderStatus.Rejected && n.Reason == reason),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockRejectedArrivesAfterOrderAlreadyConfirmed_KeepsConfirmedStatus_AndLogsWarning()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClienteNombre = "Juan Perez",
            Sku = "SKU-001",
            Cantidad = 1
        };
        order.Confirm();

        _orderRepository.GetByIdAsync(order.Id).Returns(order);

        var eventId = Guid.NewGuid();
        var command = new RejectOrderCommand(order.Id, "stock insuficiente", eventId);

        await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Null(order.RejectionReason);
        _orderRepository.DidNotReceive().Update(Arg.Any<Order>());
        await _publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());

        var warning = Assert.Single(_logger.Entries, entry => entry.Level == LogLevel.Warning);
        Assert.Contains(order.Id.ToString(), warning.Message);
        Assert.Contains(eventId.ToString(), warning.Message);
        Assert.Contains(nameof(OrderStatus.Confirmed), warning.Message);
    }

    sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));

        sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
