using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                })
            // El diagnostico interno de EF Core (Microsoft.EntityFrameworkCore.Database.Command)
            // loguea CommandError como Error incluso cuando el comando fallido es un timeout
            // transitorio ya manejado por MigrateWithRetry (contencion de locks al migrar
            // concurrentemente con Orders contra la misma instancia de SQL Server). Se baja a
            // Warning para que el retry no aparezca como excepcion no controlada en el log.
            .ConfigureWarnings(w => w.Log((RelationalEventId.CommandError, LogLevel.Warning))));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProcessedEventStore, ProcessedEventStore>();
        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();

        return services;
    }
}
