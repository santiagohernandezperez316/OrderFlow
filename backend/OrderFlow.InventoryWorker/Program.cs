using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Common;
using OrderFlow.Infrastructure.Extensions;
using OrderFlow.Infrastructure.Inventory.DataSource;
using OrderFlow.Infrastructure.Inventory.Extensions;
using OrderFlow.InventoryWorker.Consumers;
using OrderFlow.InventoryWorker.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddInventoryPersistence(config);
builder.Services.AddDomainServices(type => type.Namespace?.StartsWith("OrderFlow.Domain.Inventory", StringComparison.Ordinal) == true);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<InventoryDbContext>("inventory-db", tags: new[] { "ready" });

builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("OrderFlow.Application"));
    // Inventory Worker no registra la persistencia de Orders; excluye sus handlers del scan
    // para que la validación eager de DI (activa en Development) no falle por servicios que nunca se invocan aquí.
    cfg.TypeEvaluator = type => type.Namespace?.StartsWith("OrderFlow.Application.Orders", StringComparison.Ordinal) != true;
});

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderFlow.Infrastructure.Inventory.DataSource.InventoryDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });

    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(config["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(config["RabbitMq:Username"] ?? "guest");
            h.Password(config["RabbitMq:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MigrateAndSeedInventory();

if (builder.Configuration.GetValue<bool>("ENABLE_DIAGNOSTICS"))
{
    app.MapReplayOrderCreated();
}

app.Run();
