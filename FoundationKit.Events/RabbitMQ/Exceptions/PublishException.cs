namespace FoundationKit.Events.RabbitMQ.Exceptions;

public class PublishException : Exception
{
    public PublishException(string message, Exception e): base(message,e)
    {
        
    }
}