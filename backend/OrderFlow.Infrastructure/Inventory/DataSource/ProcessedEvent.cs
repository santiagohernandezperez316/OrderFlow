namespace OrderFlow.Infrastructure.Inventory.DataSource;

internal class ProcessedEvent
{
    public required Guid EventId { get; set; }
    public DateTime ProcessedOn { get; set; }
}
