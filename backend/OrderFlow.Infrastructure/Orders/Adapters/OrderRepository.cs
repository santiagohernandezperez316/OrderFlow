using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Orders.Entity;
using OrderFlow.Domain.Orders.Port;
using OrderFlow.Infrastructure.Orders.DataSource;

namespace OrderFlow.Infrastructure.Orders.Adapters;

internal class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order) => await context.Orders.AddAsync(order);

    public async Task<Order?> GetByIdAsync(Guid id) => await context.Orders.FirstOrDefaultAsync(order => order.Id == id);

    public async Task<IEnumerable<Order>> GetAllAsync() => await context.Orders.AsNoTracking().ToListAsync();

    public void Update(Order order) => context.Orders.Update(order);
}
