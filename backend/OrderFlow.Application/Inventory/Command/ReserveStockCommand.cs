using MediatR;

namespace OrderFlow.Application.Inventory.Command;

public record ReserveStockCommand(Guid EventId, Guid OrderId, string Sku, int Cantidad) : IRequest;
