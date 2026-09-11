using OrderFlow.Application.Ports;
using OrderFlow.Domain.Inventory.Entity;
using OrderFlow.Domain.Inventory.Port;

namespace OrderFlow.Infrastructure.Inventory.DataSource.Seed;

internal class InitializerProduct(IProductRepository productRepository, IInventoryUnitOfWork unitOfWork)
{
    public async Task CreateAsync()
    {
        var count = await productRepository.GetCountAsync();
        if (count > 0) return;

        await productRepository.AddAsync(new Product { Id = Guid.NewGuid(), Sku = "SKU-001", Name = "Teclado mecanico", Stock = 50 });
        await productRepository.AddAsync(new Product { Id = Guid.NewGuid(), Sku = "SKU-002", Name = "Mouse inalambrico", Stock = 30 });
        await productRepository.AddAsync(new Product { Id = Guid.NewGuid(), Sku = "SKU-003", Name = "Monitor 24 pulgadas", Stock = 10 });

        await unitOfWork.SaveAsync();
    }
}
