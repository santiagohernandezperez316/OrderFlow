using AutoMapper;
using MediatR;
using OrderFlow.Application.Orders.Query.Dto;
using OrderFlow.Domain.Orders.Port;

namespace OrderFlow.Application.Orders.Query;

internal class GetOrdersHandler(IOrderRepository orderRepository, IMapper mapper) : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync();
        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
