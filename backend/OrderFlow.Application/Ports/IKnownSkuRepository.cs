namespace OrderFlow.Application.Ports;

public interface IKnownSkuRepository
{
    Task<bool> ExistsAsync(string sku);
    Task<int> GetCountAsync();
    Task AddAsync(string sku);
}
