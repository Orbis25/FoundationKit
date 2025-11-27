namespace FoundationKit.Events.RabbitMQ.Handlers;

public sealed class HandlerInfo
{
    public Type HandlerType { get; set; }
    public Type MessageType { get; set; }
    
}

public sealed class HandlerContainer
{
    public List<HandlerInfo> Handlers { get; set; } = [];
}