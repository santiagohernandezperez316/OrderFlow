using MediatR;
using OrderFlow.Api.Filters;
using OrderFlow.Application.Orders.Command;
using OrderFlow.Application.Orders.Query;
using OrderFlow.Application.Orders.Query.Dto;

namespace OrderFlow.Api.ApiHandlers;

public static class OrderApi
{
    public static RouteGroupBuilder MapOrders(this IEndpointRouteBuilder routeHandler)
    {
        routeHandler.MapPost("/", async (IMediator mediator, [Validate] CreateOrderCommand command) =>
        {
            var order = await mediator.Send(command);
            return Results.Created($"/orders/{order.Id}", order);
        })
        .Produces<OrderDto>(StatusCodes.Status201Created)
        .WithSummary("Create a new order")
        .WithOpenApi();

        routeHandler.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetOrdersQuery())))
        .Produces<IEnumerable<OrderDto>>(StatusCodes.Status200OK)
        .WithSummary("Get all orders")
        .WithOpenApi();

        routeHandler.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var order = await mediator.Send(new GetOrderByIdQuery(id));
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .Produces<OrderDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Get order by id")
        .WithOpenApi();

        return (RouteGroupBuilder)routeHandler;
    }
}
