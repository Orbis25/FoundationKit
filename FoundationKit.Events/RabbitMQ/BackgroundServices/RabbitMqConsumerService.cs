using System.Reflection;
using System.Text.Json;
using FoundationKit.Events.RabbitMQ.Config;
using FoundationKit.Events.RabbitMQ.Handlers;
using FoundationKit.Events.RabbitMQ.Messages;
using Foundationkit.Extensions.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FoundationKit.Events.RabbitMQ.BackgroundServices;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerContainer _handlerContainer;
    private readonly RabbitConfig _rabbitConfig;
    private readonly RabbitTopologyRegistry _topologyRegistry;
    private IChannel _channel = null!;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger,
        IServiceProvider serviceProvider,
        HandlerContainer handlerContainer,
        RabbitConfig rabbitConfig,
        RabbitTopologyRegistry topologyRegistry)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _handlerContainer = handlerContainer;
        _rabbitConfig = rabbitConfig;
        _topologyRegistry = topologyRegistry;
    }

    // ponytail: retry loop instead of throwing, so a RabbitMQ that isn't up yet (or drops)
    // doesn't stop the whole host — only this consumer stays idle until it reconnects
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var connection = _serviceProvider.GetRequiredService<IConnection>();
                _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken)
                    .ConfigureAwait(false);

                await _channel.BasicQosAsync(0, 10, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += (model, ea) => HandleMessages(model, ea, stoppingToken);

                await InitializeQueueAndConsumerAsync(consumer, stoppingToken).ConfigureAwait(false);

                await WaitUntilCancelledAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception e) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(e, "RabbitMQ not available, retrying in {Delay}s", RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task WaitUntilCancelledAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task HandleMessages(object _, BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        try
        {
            var matchingHandlers = _handlerContainer.Handlers
                .Where(h => h.MessageType.Name == ea.BasicProperties.Type)
                .ToList();

            if (matchingHandlers.Count == 0)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                return;
            }

            var messageType = matchingHandlers[0].MessageType;
            var envelopeType = typeof(EventMessage<>).MakeGenericType(messageType);
            var @event = JsonSerializer.Deserialize(ea.Body.Span, envelopeType);

            if (@event is IEventMessage eventMessage)
            {
                using var scope = _serviceProvider.CreateScope();

                scope.ServiceProvider.GetRequiredService<MessageContext>().MessageId = eventMessage.MessageId;

                //invoke handlers
                foreach (var handlerInfo in matchingHandlers)
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

                var dlxExchange = $"{_rabbitConfig.DefaultExchange}.dlx";
                var dlqName = $"{def.QueueName}.dlq";

                // ponytail: declare here too (idempotent) so a consumer-only service works even if
                // nothing ever resolved the AddEvents IChannel singleton that also declares this
                await _channel.ExchangeDeclareAsync(def.Exchange, _rabbitConfig.DefaultExchangeType.GetDisplayName(),
                    durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.ExchangeDeclareAsync(dlxExchange, ExchangeType.Direct, durable: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.QueueBindAsync(dlqName, dlxExchange, routingKey: def.QueueName,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _channel.QueueDeclareAsync(
                    def.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        ["x-dead-letter-exchange"] = dlxExchange,
                        ["x-dead-letter-routing-key"] = def.QueueName
                    },
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error declaring consumers: Queue:{QueueName}", def.QueueName);
            }
        }
    }
}
