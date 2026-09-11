using OrderFlow.Domain.Common;

namespace OrderFlow.Infrastructure.Orders.DataSource;

internal class KnownSku : DomainEntity
{
    public required string Sku { get; set; }
}
