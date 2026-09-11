using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Tests.Inventory.Entity;

public class ProductTests
{
    [Fact]
    public void Product_WithNegativeStock_RequiredException()
    {
        var exception = Assert.Throws<RequiredException>(() =>
            new ProductDataBuilder().WithStock(-1).Build());

        Assert.Equal("stock no puede ser negativo.", exception.Message);
    }

    [Fact]
    public void TryReserve_WithEnoughStock_ReservesAndDecrementsStock()
    {
        var product = new ProductDataBuilder().WithStock(10).Build();

        var reserved = product.TryReserve(4);

        Assert.True(reserved);
        Assert.Equal(6, product.Stock);
    }

    [Fact]
    public void TryReserve_WithInsufficientStock_RejectsAndKeepsStockUnchanged()
    {
        var product = new ProductDataBuilder().WithStock(2).Build();

        var reserved = product.TryReserve(5);

        Assert.False(reserved);
        Assert.Equal(2, product.Stock);
    }
}
