using System.Text;
using System.Text.Json;
using FoundationKit.Events.RabbitMQ.Config;
using FoundationKit.Events.RabbitMQ.Exceptions;
using FoundationKit.Events.RabbitMQ.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FoundationKit.Events.RabbitMQ.Services;

public class RabbitMessageBroker: IRabbitMessageBroker
{
    private readonly ILogger<RabbitMessageBroker> _logger;
    private readonly IChannel _channel;
    private readonly RabbitConfig _rabbitConfig;
    
    public RabbitMessageBroker(IChannel channel,
        ILogger<RabbitMessageBroker> logger,
        RabbitConfig rabbitConfig)
    {
        _channel = channel;
        _logger = logger;
        _rabbitConfig = rabbitConfig;
    }

    public async Task PublishAsync<TMessage>(TMessage message, 
        string? exchangeName = null,
        string? routingKey = null, 
        CancellationToken cancellationToken = default) where TMessage : IMessage
    {
        try
        {
            var metadata = GetMetadata();
            var correlationId = metadata.CorrelationId;
            var messageId = Guid.NewGuid().ToString();
            
            var @event = new EventMessage<TMessage>(message,messageId,DateTime.UtcNow,metadata);
            var payload = JsonSerializer.SerializeToUtf8Bytes(@event);
            
            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                MessageId = messageId,
                Type = typeof(TMessage).AssemblyQualifiedName,
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = "utf-8"
            };

            routingKey = !string.IsNullOrEmpty(routingKey) ? routingKey : @event.MessageName 
                         ?? throw new InvalidOperationException("No routing key specified.");

            routingKey = $"{_rabbitConfig.QueuePrefix}:{routingKey}";
            
            var exchange = !string.IsNullOrEmpty(exchangeName) ? exchangeName : _rabbitConfig.DefaultExchange;
            
            await _channel.BasicPublishAsync(exchange, routingKey,false,props,payload, cancellationToken:cancellationToken)
                .ConfigureAwait(false);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Error publish message {ERROR}", e.InnerException?.Message ?? e.Message);
            throw new PublishException("Error trying publish a message please, check rabbitMQ connection", e);
        }
    }

    public async Task PublishAsync<TMessage>(IEnumerable<TMessage> messages, 
        string? exchangeName = null, 
        string? routingKey = null, 
        CancellationToken cancellationToken = default) where TMessage : IMessage
    {
        foreach (var message in messages)
        {
            await PublishAsync(message, exchangeName, routingKey, cancellationToken);
        }
    }

    private static EventMetadata GetMetadata() => new(Guid.NewGuid().ToString(), DateTime.UtcNow);
}