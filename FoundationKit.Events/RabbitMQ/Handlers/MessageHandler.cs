using FoundationKit.Events.RabbitMQ.Messages;

namespace FoundationKit.Events.RabbitMQ.Handlers;

public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
    public Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}