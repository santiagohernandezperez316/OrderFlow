using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Inventory.Entity;
using OrderFlow.Domain.Inventory.Port;
using OrderFlow.Infrastructure.Inventory.DataSource;

namespace OrderFlow.Infrastructure.Inventory.Adapters;

internal class ProductRepository(InventoryDbContext context) : IProductRepository
{
    public async Task<Product?> GetBySkuAsync(string sku) => await context.Products.FirstOrDefaultAsync(product => product.Sku == sku);

    public Task<int> GetCountAsync() => context.Products.CountAsync();

    public async Task AddAsync(Product product) => await context.Products.AddAsync(product);

    public void Update(Product product) => context.Products.Update(product);
}
