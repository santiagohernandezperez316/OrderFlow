using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Common;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Infrastructure.Orders.DataSource;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    internal DbSet<KnownSku> KnownSkus => Set<KnownSku>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrdersDbContext).Assembly,
            type => type.Namespace?.StartsWith("OrderFlow.Infrastructure.Orders", StringComparison.Ordinal) ?? false);

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(DomainEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).Property<DateTime>("CreatedOn");
                modelBuilder.Entity(entityType.Name).Property<DateTime>("LastModifiedOn");
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
