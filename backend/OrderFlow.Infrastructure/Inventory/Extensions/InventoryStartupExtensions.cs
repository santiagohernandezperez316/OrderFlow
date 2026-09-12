using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Ports;
using OrderFlow.Domain.Inventory.Port;
using OrderFlow.Infrastructure.Inventory.DataSource;
using OrderFlow.Infrastructure.Inventory.DataSource.Seed;

namespace OrderFlow.Infrastructure.Inventory.Extensions;

public static class InventoryStartupExtensions
{
    public static IHost MigrateAndSeedInventory(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<InventoryDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(InventoryStartupExtensions));
        MigrateWithRetry(context, logger);

        var productRepository = services.GetRequiredService<IProductRepository>();
        var unitOfWork = services.GetRequiredService<IInventoryUnitOfWork>();
        new InitializerProduct(productRepository, unitOfWork).CreateAsync().Wait();

        return host;
    }

    // Orders e Inventory comparten la misma base de datos física y ambos migran al arrancar;
    // reintenta si choca contra la creación concurrente de la base de datos por el otro servicio.
    static void MigrateWithRetry(InventoryDbContext context, ILogger logger, int maxAttempts = 5)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                context.Database.Migrate();
                return;
            }
            // SqlException.Number == -2: timeout de comando (p.ej. el ALTER DATABASE de la primera
            // migracion) por contencion de locks contra la migracion concurrente del otro servicio
            // sobre la misma instancia de SQL Server. Es transitorio y se resuelve solo en el retry.
            catch (SqlException ex) when (attempt < maxAttempts && ex.Number == -2)
            {
                logger.LogInformation("Contencion esperada durante migracion concurrente de Inventory contra la misma instancia de SQL Server (timeout, intento {Attempt}/{MaxAttempts}). Reintentando en 5 segundos.", attempt, maxAttempts);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            catch (SqlException ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Intento {Attempt}/{MaxAttempts} de migrar la base de datos de Inventory fallo. Reintentando en 5 segundos.", attempt, maxAttempts);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
