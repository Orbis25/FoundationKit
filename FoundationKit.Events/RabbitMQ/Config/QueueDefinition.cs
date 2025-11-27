namespace FoundationKit.Events.RabbitMQ.Config;

public class RabbitTopologyRegistry
{
    public IEnumerable<QueueDefinition> Queues { get; }
    
    public RabbitTopologyRegistry(IEnumerable<QueueDefinition> queues)
    {
        Queues = queues;
    }
}
public record QueueDefinition(string Exchange, string RoutingKey, string QueueName);