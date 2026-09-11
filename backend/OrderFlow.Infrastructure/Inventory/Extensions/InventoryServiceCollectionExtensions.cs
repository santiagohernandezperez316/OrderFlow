using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Inventory.Port;
using OrderFlow.Infrastructure.Inventory.Adapters;
using OrderFlow.Infrastructure.Inventory.DataSource;

namespace OrderFlow.Infrastructure.Inventory.Extensions;

public static class InventoryServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("InventoryDb"),
                sql =>
                {
                    sql.EnableRetryOnFailure();
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory");
                }));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProcessedEventStore, ProcessedEventStore>();
        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();

        return services;
    }
}
