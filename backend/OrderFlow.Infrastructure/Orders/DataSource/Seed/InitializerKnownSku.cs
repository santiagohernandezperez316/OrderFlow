using OrderFlow.Application.Ports;

namespace OrderFlow.Infrastructure.Orders.DataSource.Seed;

internal class InitializerKnownSku(IKnownSkuRepository knownSkuRepository, IOrdersUnitOfWork unitOfWork)
{
    private static readonly string[] Skus = ["SKU-001", "SKU-002", "SKU-003"];

    public async Task CreateAsync()
    {
        var count = await knownSkuRepository.GetCountAsync();
        if (count > 0) return;

        foreach (var sku in Skus)
        {
            await knownSkuRepository.AddAsync(sku);
        }

        await unitOfWork.SaveAsync();
    }
}
