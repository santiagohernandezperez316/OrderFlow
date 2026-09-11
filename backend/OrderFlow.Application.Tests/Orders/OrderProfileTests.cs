using AutoMapper;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Query.Dto;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Application.Tests.Orders;

public class OrderProfileTests
{
    static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>()).CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_OrderToOrderDto_MapsCreadoEnFromCreatedOn()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClienteNombre = "Cliente Uno",
            Sku = "SKU-001",
            Cantidad = 3
        };
        typeof(Order).GetProperty(nameof(Order.CreatedOn))!.SetValue(order, new DateTime(2026, 9, 11, 10, 0, 0, DateTimeKind.Utc));

        var dto = CreateMapper().Map<OrderDto>(order);

        Assert.NotNull(dto);
        Assert.Equal(order.Id, dto.Id);
        Assert.Equal(order.ClienteNombre, dto.ClienteNombre);
        Assert.Equal(order.Sku, dto.Sku);
        Assert.Equal(order.Cantidad, dto.Cantidad);
        Assert.Equal(OrderStatus.Pending, dto.Status);
        Assert.Equal(new DateTime(2026, 9, 11, 10, 0, 0, DateTimeKind.Utc), dto.CreadoEn);
    }
}
