using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Common;
using OrderFlow.Infrastructure.Inventory.DataSource;

namespace OrderFlow.Infrastructure.Inventory.Adapters;

internal class InventoryUnitOfWork(InventoryDbContext context) : IInventoryUnitOfWork
{
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        EntityTimestamps.Apply(context);
        await context.SaveChangesAsync(cancellationToken);
    }
}
