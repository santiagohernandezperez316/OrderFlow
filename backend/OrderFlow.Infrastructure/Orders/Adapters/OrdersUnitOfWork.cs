using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Common;
using OrderFlow.Infrastructure.Orders.DataSource;

namespace OrderFlow.Infrastructure.Orders.Adapters;

internal class OrdersUnitOfWork(OrdersDbContext context) : IOrdersUnitOfWork
{
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        EntityTimestamps.Apply(context);
        await context.SaveChangesAsync(cancellationToken);
    }
}
