namespace FoundationKit.Events.RabbitMQ.Handlers;

public sealed class HandlerInfo
{
    public Type HandlerType { get; set; } = null!;
    public Type MessageType { get; set; } = null!;
    
}

public sealed class HandlerContainer
{
    public List<HandlerInfo> Handlers { get; set; } = [];
}