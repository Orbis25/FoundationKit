using System.Reflection;
using FoundationKit.Events.RabbitMQ.BackgroundServices;
using FoundationKit.Events.RabbitMQ.Config;
using FoundationKit.Events.RabbitMQ.Handlers;
using FoundationKit.Events.RabbitMQ.Messages;
using FoundationKit.Events.RabbitMQ.Services;
using Foundationkit.Extensions.Enums;
using Microsoft.Extensions.DependencyModel;
using RabbitMQ.Client;

namespace FoundationKit.Events.Extensions;

public static class EventExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services, 
        RabbitConfig configuration)
    {
        var connectionFactory = GetConnectionFactory(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<RabbitTopologyRegistry>();

        // ponytail: lazy connect, only touches RabbitMQ the first time something publishes or consumes,
        // so a stopped broker doesn't block app startup or unrelated requests
        services.AddSingleton<IConnection>(_ =>
            connectionFactory.CreateConnectionAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        services.AddSingleton<IChannel>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var channel = connection.CreateChannelAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var exchangeType = configuration.DefaultExchangeType.GetDisplayName();
            channel.ExchangeDeclareAsync(configuration.DefaultExchange, exchangeType)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            return channel;
        });

        services.AddScoped<IRabbitMessageBroker,RabbitMessageBroker>();
        services.AddScoped<MessageContext>();

        services.RegisterConsumers();
        
        return services;
    }

    public static IServiceCollection AddSubscriber<T>(this IServiceCollection services, string? exchange = null) where T: IMessage
    {
        services.AddSingleton<QueueDefinition>(sp => 
        {
            var config = sp.GetRequiredService<RabbitConfig>();
            var typeName = typeof(T).Name;
            var finalExchange = !string.IsNullOrEmpty(exchange) ? exchange : config.DefaultExchange;
            var queueName = $"{config.QueuePrefix}:{typeName}";

            return new(finalExchange, typeName, queueName);
        });
        
        return services;
    }

    private static ConnectionFactory GetConnectionFactory(RabbitConfig configuration)
    {
        ConnectionFactory connectionFactory = new();
        
        if (!string.IsNullOrEmpty(configuration.Url))
        {
            connectionFactory.Uri = new Uri(configuration.Url);
            return connectionFactory;
        }
            
        connectionFactory.UserName = configuration.User ?? "guest";
        connectionFactory.Password = configuration.Password ?? "guest";
        connectionFactory.HostName = configuration.Host ?? "localhost";
        connectionFactory.Port = configuration.Port != 0 ? configuration.Port : AmqpTcpEndpoint.UseDefaultPort;

        return connectionFactory;

    }
    
    private static IServiceCollection RegisterConsumers(this IServiceCollection services)
    {
        var messageType = typeof(IMessageHandler<>);
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        
        var deps = DependencyContext.Default?.RuntimeLibraries 
                   ?? Enumerable.Empty<RuntimeLibrary>();

        // load referenced assemblies
        foreach (var lib in deps)
        {
            try
            {
                if (assemblies.Any(a => 
                        string.Equals(a.GetName().Name, 
                            lib.Name, 
                            StringComparison.OrdinalIgnoreCase)))
                    continue;

                var asm = Assembly.Load(new AssemblyName(lib.Name));
                assemblies.Add(asm);
            }
            catch
            {
                // ignored
            }
        }
        var result = new List<HandlerInfo>();
        foreach (var asm in assemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }
            catch
            {
                continue;
            }

            // Find concrete classes that implement a closed generic of IMessageHandler<>
            var handlers = (from t in 
                types.Where(t => t is { IsClass: true, IsAbstract: false }) 
                let interfaces = t!.GetInterfaces() 
                let matched = interfaces.Where(i => i.IsGenericType 
                                                    && i.GetGenericTypeDefinition() == messageType).ToList() 
                where matched.Any() from m in matched 
                let msgType = m.GetGenericArguments().First() 
                select new HandlerInfo { HandlerType = t!, MessageType = msgType }).ToList();

            // Register each handler type in the DI container
            foreach (var handler in handlers)
            {
                services.AddScoped(handler.HandlerType);
            }
            
            result.AddRange(handlers.Select(h => new HandlerInfo { HandlerType = h.HandlerType, MessageType = h.MessageType }));
        }
        
        // Register the message types in a container
        var container = new HandlerContainer()
        {
            Handlers = result
        };
        
        services.AddSingleton(container);

        services.AddHostedService<RabbitMqConsumerService>();
        
        return services;
    }
}