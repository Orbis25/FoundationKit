namespace FoundationKit.Events.RabbitMQ.Exceptions;

public class DeclareConsumerException(string message, Exception ex) : Exception(message,ex);