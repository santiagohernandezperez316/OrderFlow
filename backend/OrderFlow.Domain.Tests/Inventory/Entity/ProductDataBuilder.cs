using OrderFlow.Domain.Inventory.Entity;

namespace OrderFlow.Domain.Tests.Inventory.Entity;

public class ProductDataBuilder
{
    Guid _id = Guid.NewGuid();
    string _sku = "SKU-001";
    string _name = "Teclado mecanico";
    int _stock = 10;

    public ProductDataBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductDataBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductDataBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public Product Build()
    {
        return new Product
        {
            Id = _id,
            Sku = _sku,
            Name = _name,
            Stock = _stock
        };
    }
}
