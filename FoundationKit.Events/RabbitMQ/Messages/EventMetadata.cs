namespace FoundationKit.Events.RabbitMQ.Messages;

public record EventMetadata(string? CorrelationId, DateTime CreatedAt);