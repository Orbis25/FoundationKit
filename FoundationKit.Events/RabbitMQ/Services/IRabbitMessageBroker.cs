using FoundationKit.Events.RabbitMQ.Messages;

namespace FoundationKit.Events.RabbitMQ.Services;

public interface IRabbitMessageBroker
{
    public Task PublishAsync<TMessage>(TMessage message,
        string? exchangeName = null,
        string? routingKey = null,
        CancellationToken cancellationToken = default) where TMessage : IMessage;

    Task PublishAsync<TMessage>(IEnumerable<TMessage> messages,
        string? exchangeName = null,
        string? routingKey = null,
        CancellationToken cancellationToken = default) where TMessage : IMessage;
}