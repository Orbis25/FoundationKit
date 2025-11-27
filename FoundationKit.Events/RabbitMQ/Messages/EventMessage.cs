namespace FoundationKit.Events.RabbitMQ.Messages;

public sealed class EventMessage<T> : IEventMessage<T> where T : IMessage
{
    public string? MessageId { get; }
    public string? MessageName { get; }
    public DateTime CreatedAt { get; }
    
    public object GetData() => Data;
    public EventMetadata EventMetadata { get; }
    public T Data { get; }

    public EventMessage(T data,string messageId, DateTime createdAt, EventMetadata eventMetadata)
    {
        Data = data;
        MessageId = messageId;
        CreatedAt = createdAt;
        EventMetadata = eventMetadata;
        MessageName = typeof(T).Name;
    }
}