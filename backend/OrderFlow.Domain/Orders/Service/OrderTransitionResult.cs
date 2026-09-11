using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Domain.Orders.Service;

public readonly record struct OrderTransitionResult(bool Transitioned, OrderStatus? CurrentStatus);
