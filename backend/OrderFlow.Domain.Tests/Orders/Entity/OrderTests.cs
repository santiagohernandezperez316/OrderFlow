using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Domain.Tests.Orders.Entity;

public class OrderTests
{
    [Fact]
    public void Order_WithCantidadZero_RequiredException()
    {
        var exception = Assert.Throws<RequiredException>(() =>
            new OrderDataBuilder().WithCantidad(0).Build());

        Assert.Equal("cantidad debe estar entre 1 y 100.", exception.Message);
    }

    [Fact]
    public void Order_WithCantidadOverMaximun_RequiredException()
    {
        var exception = Assert.Throws<RequiredException>(() =>
            new OrderDataBuilder().WithCantidad(101).Build());

        Assert.Equal("cantidad debe estar entre 1 y 100.", exception.Message);
    }

    [Fact]
    public void Order_WithClienteNombreEmpty_RequiredException()
    {
        var exception = Assert.Throws<RequiredException>(() =>
            new OrderDataBuilder().WithClienteNombre(string.Empty).Build());

        Assert.Equal("clienteNombre no debe estar vacio.", exception.Message);
    }

    [Fact]
    public void Confirm_FromPending_TransitionsToConfirmed()
    {
        var order = new OrderDataBuilder().Build();

        var transitioned = order.Confirm();

        Assert.True(transitioned);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_IsIdempotentNoOp()
    {
        var order = new OrderDataBuilder().Build();
        order.Confirm();

        var transitionedAgain = order.Confirm();

        Assert.False(transitionedAgain);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Reject_WhenAlreadyConfirmed_IsIdempotentNoOp()
    {
        var order = new OrderDataBuilder().Build();
        order.Confirm();

        var rejected = order.Reject();

        Assert.False(rejected);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}
