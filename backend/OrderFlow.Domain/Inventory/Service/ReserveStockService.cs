using OrderFlow.Domain.Common;
using OrderFlow.Domain.Inventory.Port;

namespace OrderFlow.Domain.Inventory.Service;

[DomainService]
public class ReserveStockService(IProductRepository productRepository)
{
    public async Task<bool> ExecuteAsync(string sku, int cantidad)
    {
        var product = await productRepository.GetBySkuAsync(sku);
        if (product is null)
        {
            return false;
        }

        var reserved = product.TryReserve(cantidad);
        if (reserved)
        {
            productRepository.Update(product);
        }

        return reserved;
    }
}
