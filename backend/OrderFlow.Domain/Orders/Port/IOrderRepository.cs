using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Domain.Orders.Port;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    void Update(Order order);
}
