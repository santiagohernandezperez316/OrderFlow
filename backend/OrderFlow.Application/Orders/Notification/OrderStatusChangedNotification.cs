using MediatR;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Application.Orders.Notification;

public record OrderStatusChangedNotification(Guid OrderId, OrderStatus Status, string? Reason = null) : INotification;
