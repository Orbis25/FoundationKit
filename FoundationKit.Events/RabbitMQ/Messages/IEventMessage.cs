namespace FoundationKit.Events.RabbitMQ.Messages;

public interface IEventMessage
{
    string MessageId { get; }
    string MessageName { get; }
    DateTime CreatedAt { get; }

    object GetData(); 
}

public interface IEventMessage<out T> : IEventMessage where T : IMessage
{
    public T Data { get; }
}