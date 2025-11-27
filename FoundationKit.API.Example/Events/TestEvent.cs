using FoundationKit.Events.RabbitMQ.Messages;

namespace FoundationKit.API.Example.Events;

public class TestEvent : IMessage
{
    public string? Name { get; set; }
}