using OrderFlow.Api.ApiHandlers;
using OrderFlow.Application.Orders.Command;

namespace OrderFlow.Api.Tests.ApiHandlers;

public class CreateOrderCommandValidatorTests
{
    readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithClienteNombreEmpty_IsInvalid()
    {
        var result = _validator.Validate(new CreateOrderCommand(string.Empty, "SKU-001", 1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderCommand.ClienteNombre));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithCantidadOutsideAllowedRange_IsInvalid(int cantidad)
    {
        var result = _validator.Validate(new CreateOrderCommand("Cliente Uno", "SKU-001", cantidad));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderCommand.Cantidad));
    }

    [Fact]
    public void Validate_WithValidData_IsValid()
    {
        var result = _validator.Validate(new CreateOrderCommand("Cliente Uno", "SKU-001", 1));

        Assert.True(result.IsValid);
    }
}
