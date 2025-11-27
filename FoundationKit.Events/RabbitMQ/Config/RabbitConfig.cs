
using System.Reflection;
using FoundationKit.Events.RabbitMQ.ValueObjects;

namespace FoundationKit.Events.RabbitMQ.Config;

public class RabbitConfig
{
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Url { get; set; }
    public required string DefaultExchange { get; set; }
    
    public ExchangeType DefaultExchangeType { get; set; } = ExchangeType.Topic;
    public bool RedeliverUnackedMessages { get; set; } = true;
    public required string? QueuePrefix { get; set; }
}