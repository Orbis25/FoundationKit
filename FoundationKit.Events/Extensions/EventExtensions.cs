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
        var conn =  connectionFactory.CreateConnectionAsync().Result;
        var channel = conn.CreateChannelAsync().Result;
        Console.WriteLine("RabbitMQ Connected");

        services.AddSingleton(conn);
        services.AddSingleton(channel);
        services.AddSingleton(configuration);
        
        services.AddScoped<IRabbitMessageBroker,RabbitMessageBroker>();
        
        DeclareExchangeAsync(channel, configuration).GetAwaiter().GetResult();
        
        services.RegisterConsumers();
        
        return services;
    }

    public static IServiceCollection AddSubscriber<T>(this IServiceCollection services, string? exchange = null) where T: IMessage
    {
        var configuration = services.BuildServiceProvider().GetService<RabbitConfig>();
        var channel = services.BuildServiceProvider().GetService<IChannel>();
        
        var name = typeof(T).Name;
        try
        {
            exchange = !string.IsNullOrEmpty(exchange) ? exchange : configuration?.DefaultExchange ?? "DEAD-EXCHANGE";

            name = $"{configuration?.QueuePrefix}:{name}";
            channel.QueueDeclareAsync(name,false, false, false, null)
                .GetAwaiter().GetResult();
            
            channel.QueueBindAsync(name, exchange,name, null)
                .GetAwaiter().GetResult();
            
            Console.WriteLine($"Subscriber registered for {name}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error registering subscriber: " + e.Message + $"-> name:" + name);
        }
        
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
    
    private static Task DeclareExchangeAsync(IChannel channel, RabbitConfig configuration)
    {
        var type = configuration.DefaultExchangeType.GetDisplayName();
        return Task.Run(() =>
        {
            channel.ExchangeDeclareAsync(exchange: configuration.DefaultExchange, type);
        });
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