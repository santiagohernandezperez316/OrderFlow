using AutoMapper;
using MediatR;
using OrderFlow.Application.Orders.Query.Dto;
using OrderFlow.Domain.Orders.Port;

namespace OrderFlow.Application.Orders.Query;

internal class GetOrderByIdHandler(IOrderRepository orderRepository, IMapper mapper) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);
        return order is null ? null : mapper.Map<OrderDto>(order);
    }
}
