using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Inventory.Entity;

public class Product : DomainEntity
{
    private string _sku = default!;
    private string _name = default!;
    private int _stock;

    public required string Sku
    {
        get => _sku;
        set
        {
            value.ValidateRequired("sku no debe estar vacio.");
            _sku = value;
        }
    }

    public required string Name
    {
        get => _name;
        set
        {
            value.ValidateRequired("name no debe estar vacio.");
            _name = value;
        }
    }

    public required int Stock
    {
        get => _stock;
        set
        {
            value.ValidateGreaterOrEqualThanZero("stock no puede ser negativo.");
            _stock = value;
        }
    }

    public bool TryReserve(int cantidad)
    {
        if (Stock < cantidad)
        {
            return false;
        }

        _stock -= cantidad;
        return true;
    }
}
