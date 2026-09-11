namespace OrderFlow.Application.Ports;

public interface IProcessedEventStore
{
    Task<bool> HasProcessedAsync(Guid eventId);
    Task MarkProcessedAsync(Guid eventId);
}
