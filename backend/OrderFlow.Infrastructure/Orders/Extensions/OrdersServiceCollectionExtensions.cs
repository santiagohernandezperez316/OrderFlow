using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Orders.Command.Factory;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Orders.Port;
using OrderFlow.Infrastructure.Orders.Adapters;
using OrderFlow.Infrastructure.Orders.DataSource;

namespace OrderFlow.Infrastructure.Orders.Extensions;

public static class OrdersServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("OrdersDb"),
                sql =>
                {
                    sql.EnableRetryOnFailure();
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "orders");
                })
            // El diagnostico interno de EF Core (Microsoft.EntityFrameworkCore.Database.Command)
            // loguea CommandError como Error incluso cuando el comando fallido es un timeout
            // transitorio ya manejado por MigrateWithRetry (contencion de locks al migrar
            // concurrentemente con Inventory contra la misma instancia de SQL Server). Se baja a
            // Warning para que el retry no aparezca como excepcion no controlada en el log.
            .ConfigureWarnings(w => w.Log((RelationalEventId.CommandError, LogLevel.Warning))));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IKnownSkuRepository, KnownSkuRepository>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddTransient<OrderFactory>();

        return services;
    }
}
