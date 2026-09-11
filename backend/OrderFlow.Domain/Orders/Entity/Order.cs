using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Orders.Entity;

public class Order : DomainEntity
{
    private string _clienteNombre = default!;
    private string _sku = default!;
    private int _cantidad;

    public required string ClienteNombre
    {
        get => _clienteNombre;
        set
        {
            value.ValidateRequired("clienteNombre no debe estar vacio.");
            _clienteNombre = value;
        }
    }

    public required string Sku
    {
        get => _sku;
        set
        {
            value.ValidateRequired("sku no debe estar vacio.");
            _sku = value;
        }
    }

    public required int Cantidad
    {
        get => _cantidad;
        set
        {
            value.ValidateRange(1, 100, "cantidad debe estar entre 1 y 100.");
            _cantidad = value;
        }
    }

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public string? RejectionReason { get; private set; }

    public DateTime CreatedOn { get; private set; }

    public bool Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            return false;
        }

        Status = OrderStatus.Confirmed;
        return true;
    }

    public bool Reject(string? reason = null)
    {
        if (Status != OrderStatus.Pending)
        {
            return false;
        }

        Status = OrderStatus.Rejected;
        RejectionReason = reason;
        return true;
    }
}
