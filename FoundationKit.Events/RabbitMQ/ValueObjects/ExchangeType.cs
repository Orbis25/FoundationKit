using System.ComponentModel.DataAnnotations;

namespace FoundationKit.Events.RabbitMQ.ValueObjects;

public enum ExchangeType
{
    [Display(Name = "direct")]
    Direct,
    [Display(Name = "fanout")]
    Fanout,
    [Display(Name = "topic")]
    Topic,
    [Display(Name = "headers")]
    Headers
}