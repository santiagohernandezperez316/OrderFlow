using OrderFlow.Api.ApiHandlers;
using OrderFlow.Api.Consumers;
using OrderFlow.Api.Filters;
using OrderFlow.Api.Hubs;
using OrderFlow.Api.Middleware;
using OrderFlow.Api.Swagger;
using OrderFlow.Application.Ports;
using OrderFlow.Infrastructure.Common;
using OrderFlow.Infrastructure.Extensions;
using OrderFlow.Infrastructure.Orders.DataSource;
using OrderFlow.Infrastructure.Orders.Extensions;
using FluentValidation;
using MassTransit;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Debugging;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services.AddOrdersPersistence(config);
builder.Services.AddDomainServices(type => type.Namespace?.StartsWith("OrderFlow.Domain.Orders", StringComparison.Ordinal) == true);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("orders-db", tags: new[] { "ready" });

builder.Services.AddAutoMapper(Assembly.Load("OrderFlow.Application"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SchemaFilter<EnumSchemaFilter>());

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<IOrderRealtimeNotifier, SignalROrderRealtimeNotifier>();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

const string FrontendCorsPolicy = "FrontendCors";
var frontendOrigin = config["FRONTEND_ORIGIN"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (!string.IsNullOrWhiteSpace(frontendOrigin))
        {
            policy.WithOrigins(frontendOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("OrderFlow.Application"));
    // Orders API no registra la persistencia de Inventory; excluye sus handlers del scan
    // para que la validación eager de DI (activa en Development) no falle por servicios que nunca se invocan aquí.
    cfg.TypeEvaluator = type => type.Namespace?.StartsWith("OrderFlow.Application.Inventory", StringComparison.Ordinal) != true;
});

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrdersDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });

    x.AddConsumer<StockReservedConsumer>();
    x.AddConsumer<StockRejectedConsumer>();

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

builder.Host.UseSerilog((context, loggerconfiguration) =>
    loggerconfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

SelfLog.Enable(Console.Error);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AppExceptionHandlerMiddleware>();

app.UseCors(FrontendCorsPolicy);

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

app.MapGroup("/orders")
    .MapOrders()
    .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory)
    .WithTags("Orders");

app.MapHub<OrdersHub>("/hubs/orders");

app.MigrateAndSeedOrders();

app.Run();
