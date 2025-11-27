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
    private readonly RabbitTopologyRegistry _topologyRegistry;
    private IChannel _channel;

    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger,
        IConnection connection,
        IServiceProvider serviceProvider,
        HandlerContainer handlerContainer,
        RabbitConfig rabbitConfig,
        RabbitTopologyRegistry topologyRegistry)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _handlerContainer = handlerContainer;
        _rabbitConfig = rabbitConfig;
        _topologyRegistry = topologyRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.Run(async () =>
    {
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken)
                .ConfigureAwait(false);

            await _channel.BasicQosAsync(0, 10, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await HandleMessages(model, ea, stoppingToken).ConfigureAwait(false);
            };

            await InitializeQueueAndConsumerAsync(consumer, stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in RabbitMqConsumerService");
            throw new ConsumerServiceException("Error creating a consumer channel please, check rabbitMQ connection",
                e);
        }
    }, stoppingToken);


    private async Task HandleMessages(object _, BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageType = Type.GetType(ea.BasicProperties.Type ?? string.Empty);
            if (messageType == null)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                return;
            }

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
            await _channel.BasicNackAsync(ea.DeliveryTag, false, _rabbitConfig.RedeliverUnackedMessages,
                cancellationToken);
        }
    }

    private async Task InitializeQueueAndConsumerAsync(AsyncEventingBasicConsumer consumer,
        CancellationToken cancellationToken = default)
    {
        foreach (var def in _topologyRegistry.Queues)
        {
            try
            {
                _logger.LogInformation("Declaring queue: {QueueName}", def.QueueName);

                await _channel.QueueDeclareAsync(
                    def.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.QueueBindAsync(
                    queue: def.QueueName,
                    exchange: def.Exchange,
                    routingKey: def.RoutingKey,
                    arguments: null,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.BasicConsumeAsync(
                        queue: def.QueueName,
                        autoAck: false,
                        consumer: consumer,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                _logger.LogError("Error declaring consumers: Queue:{ErrorMessage}", def.QueueName);
            }
        }
    }
}