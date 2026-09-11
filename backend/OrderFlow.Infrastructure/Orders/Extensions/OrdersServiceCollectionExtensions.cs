using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                }));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IKnownSkuRepository, KnownSkuRepository>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddTransient<OrderFactory>();

        return services;
    }
}
