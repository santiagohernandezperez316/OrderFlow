using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Infrastructure.Inventory.DataSource;

// Solo la usa la herramienta "dotnet ef" al generar migraciones; en runtime la app registra el DbContext via DI.
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=OrderFlowDb;User Id=sa;Password=DesignTime!Passw0rd;TrustServerCertificate=True",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory"));

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
