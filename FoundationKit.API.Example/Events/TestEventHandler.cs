using FoundationKit.Events.RabbitMQ.Handlers;

namespace FoundationKit.API.Example.Events;

public class TestEventHandler : IMessageHandler<TestEvent>
{
    private readonly ILogger<TestEventHandler> _logger;

    public TestEventHandler(ILogger<TestEventHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(TestEvent message, CancellationToken cancellationToken = default)
    {
        try
        {
             _logger.LogInformation("Handling TestEvent with Name: {Name}", message.Name);
        }
        catch (Exception e)
        {
            _logger.LogError("Error handling TestEvent: {Message}", e.Message);
        }
    }
}