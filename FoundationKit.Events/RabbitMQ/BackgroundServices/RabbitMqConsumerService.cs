using System.Reflection;
using System.Text.Json;
using FoundationKit.Events.RabbitMQ.Config;
using FoundationKit.Events.RabbitMQ.Exceptions;
using FoundationKit.Events.RabbitMQ.Handlers;
using FoundationKit.Events.RabbitMQ.Messages;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FoundationKit.Events.RabbitMQ.BackgroundServices;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly HandlerContainer _handlerContainer;
    private readonly RabbitConfig _rabbitConfig;
    private IChannel _channel;
    
    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger,
        IConnection connection,
        IServiceProvider serviceProvider,
        HandlerContainer handlerContainer,
        RabbitConfig rabbitConfig)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _handlerContainer = handlerContainer;
        _rabbitConfig = rabbitConfig;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.Run(async() =>
    {
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken:stoppingToken)
                .ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
               await HandleMessages(model, ea, stoppingToken).ConfigureAwait(false);
            };
            
            await DeclareConsumersAsync(consumer, stoppingToken).ConfigureAwait(false);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in RabbitMqConsumerService");
            throw new ConsumerServiceException("Error creating a consumer channel please, check rabbitMQ connection", e);
            
        }
    }, stoppingToken);


    private async Task HandleMessages(object _, BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageType = Type.GetType(ea.BasicProperties.Type ?? string.Empty);
            if(messageType == null) return;
            
            var envelopeType = typeof(EventMessage<>).MakeGenericType(messageType);
            var @event = JsonSerializer.Deserialize(ea.Body.Span, envelopeType);

            if (@event is IEventMessage eventMessage)
            {
                using var scope = _serviceProvider.CreateScope();
                
                //invoke handlers
                foreach (var handlerInfo in _handlerContainer.Handlers
                             .Where(h => h.MessageType == messageType))
                {
                    
                    var handler = scope.ServiceProvider.GetRequiredService(handlerInfo.HandlerType);
                    var method = handlerInfo.HandlerType.GetMethod("HandleAsync",
                        BindingFlags.Instance | BindingFlags.Public);

                    if (method == null) continue;
                    
                    var task = (Task?)method.Invoke(handler, [@eventMessage.GetData(), cancellationToken]);
                    if (task != null)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                    
                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error handling message: {ErrorMessage}", e.Message);
            await _channel.BasicNackAsync(ea.DeliveryTag, false, _rabbitConfig.RedeliverUnackedMessages, cancellationToken);
        }
    }

    private async Task DeclareConsumersAsync(AsyncEventingBasicConsumer consumer, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var queueName in _handlerContainer.Handlers
                         .Select(handlerInfo => handlerInfo.MessageType)
                         .Select(messageType => messageType.Name))
            {
                await _channel.BasicConsumeAsync(
                        queue: $"{_rabbitConfig.QueuePrefix}:{queueName}",
                        autoAck: false,
                        consumer: consumer,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error declaring consumers: {ErrorMessage}", e.Message);
            throw new DeclareConsumerException("Error declaring consumers", e);
        }
    }
}