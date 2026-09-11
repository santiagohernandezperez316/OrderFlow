using OrderFlow.Domain.Inventory.Entity;

namespace OrderFlow.Domain.Inventory.Port;

public interface IProductRepository
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<int> GetCountAsync();
    Task AddAsync(Product product);
    void Update(Product product);
}
