using MassTransit;
using OrderFlow.Application.Ports;

namespace OrderFlow.Infrastructure.Common;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class
        => publishEndpoint.Publish(@event, cancellationToken);
}
