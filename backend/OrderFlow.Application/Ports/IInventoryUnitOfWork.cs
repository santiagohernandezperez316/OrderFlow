namespace OrderFlow.Application.Ports;

public interface IInventoryUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
