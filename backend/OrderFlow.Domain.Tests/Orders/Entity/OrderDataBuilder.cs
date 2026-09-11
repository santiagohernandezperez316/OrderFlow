using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Domain.Tests.Orders.Entity;

public class OrderDataBuilder
{
    Guid _id = Guid.NewGuid();
    string _clienteNombre = "Juan Perez";
    string _sku = "SKU-001";
    int _cantidad = 1;

    public OrderDataBuilder WithClienteNombre(string clienteNombre)
    {
        _clienteNombre = clienteNombre;
        return this;
    }

    public OrderDataBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public OrderDataBuilder WithCantidad(int cantidad)
    {
        _cantidad = cantidad;
        return this;
    }

    public Order Build()
    {
        return new Order
        {
            Id = _id,
            ClienteNombre = _clienteNombre,
            Sku = _sku,
            Cantidad = _cantidad
        };
    }
}
