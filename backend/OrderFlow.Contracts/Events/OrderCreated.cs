namespace OrderFlow.Contracts.Events;

public record OrderCreated(Guid OrderId, string Sku, int Cantidad);
