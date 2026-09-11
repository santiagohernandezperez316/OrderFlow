using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Orders.DataSource;

namespace OrderFlow.Infrastructure.Orders.Adapters;

internal class KnownSkuRepository(OrdersDbContext context) : IKnownSkuRepository
{
    public async Task<bool> ExistsAsync(string sku) =>
        await context.KnownSkus.AsNoTracking().AnyAsync(knownSku => knownSku.Sku == sku);

    public Task<int> GetCountAsync() => context.KnownSkus.CountAsync();

    public async Task AddAsync(string sku) =>
        await context.KnownSkus.AddAsync(new KnownSku { Id = Guid.NewGuid(), Sku = sku });
}
