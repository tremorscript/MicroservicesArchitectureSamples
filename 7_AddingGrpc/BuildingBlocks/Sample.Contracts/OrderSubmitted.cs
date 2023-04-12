using MassTransit;

namespace Sample.Contracts;

public class OrderSubmitted
{
    private Guid OrderId { get; }
    private DateTime Timestamp { get; }
    private string CustomerNumber { get; }
    private MessageData<string> Notes { get; }
}