namespace FoundationKit.Events.RabbitMQ.Exceptions;

public class ConsumerServiceException : Exception
{
    public ConsumerServiceException(string message, Exception e):base(message,e)
    {
        
    }
}