using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Inventory.DataSource;

namespace OrderFlow.Infrastructure.Inventory.Adapters;

internal class ProcessedEventStore(InventoryDbContext context) : IProcessedEventStore
{
    public async Task<bool> HasProcessedAsync(Guid eventId) =>
        await context.ProcessedEvents.AsNoTracking().AnyAsync(processedEvent => processedEvent.EventId == eventId);

    public async Task MarkProcessedAsync(Guid eventId) =>
        await context.ProcessedEvents.AddAsync(new ProcessedEvent { EventId = eventId, ProcessedOn = DateTime.UtcNow });
}
