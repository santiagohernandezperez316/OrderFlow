using AutoMapper;
using MediatR;
using OrderFlow.Application.Orders.Command.Factory;
using OrderFlow.Application.Orders.Query.Dto;
using OrderFlow.Application.Ports;
using OrderFlow.Contracts.Events;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Orders.Service;

namespace OrderFlow.Application.Orders.Command;

internal class CreateOrderHandler(
    OrderFactory orderFactory,
    CreateOrderService createOrderService,
    IKnownSkuRepository knownSkuRepository,
    IOrdersUnitOfWork unitOfWork,
    IEventPublisher eventPublisher,
    IMapper mapper) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var skuExists = await knownSkuRepository.ExistsAsync(request.Sku);
        if (!skuExists)
        {
            throw new SkuNotFoundException($"el sku '{request.Sku}' no existe.");
        }

        var order = orderFactory.Create(request);
        await createOrderService.ExecuteAsync(order);

        await eventPublisher.PublishAsync(new OrderCreated(order.Id, order.Sku, order.Cantidad), cancellationToken);

        await unitOfWork.SaveAsync(cancellationToken);

        return mapper.Map<OrderDto>(order);
    }
}
