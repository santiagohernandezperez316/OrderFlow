namespace OrderFlow.Application.Ports;

public interface IOrdersUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
