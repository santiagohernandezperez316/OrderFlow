using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrderFlow.Contracts.Events;

namespace OrderFlow.InventoryWorker.Diagnostics;

// SOLO PARA PRUEBAS MANUALES — no es parte del contrato de la prueba técnica.
// Permite republicar un OrderCreated con un EventId elegido a mano para verificar
// en vivo que el segundo consumo con el mismo EventId no vuelve a descontar stock
// (ver ProcessedEvents / Products en la base de datos). Activo solo si ENABLE_DIAGNOSTICS=true.
public static class ReplayOrderCreatedEndpoint
{
    public static void MapReplayOrderCreated(this IEndpointRouteBuilder app)
    {
        // Publica vía IBus (no IPublishEndpoint): IPublishEndpoint queda envuelto por el bus outbox
        // a nivel de scope y solo se libera cuando esa misma request hace SaveChanges en el DbContext,
        // cosa que este endpoint de diagnóstico nunca hace. IBus entrega directo al broker.
        app.MapPost("/diagnostics/replay-order-created", async (IBus bus, ReplayOrderCreatedRequest request) =>
        {
            await bus.Publish<OrderCreated>(
                new OrderCreated(request.OrderId, request.Sku, request.Cantidad),
                context => context.MessageId = request.EventId);

            return Results.Accepted();
        });
    }
}

public record ReplayOrderCreatedRequest(Guid EventId, Guid OrderId, string Sku, int Cantidad);
