using MassTransit;

namespace Sample.Contracts;

public interface SubmitOrder
{
    Guid OrderId { get; }
    DateTime Timestamp { get; }

    string CustomerNumber { get; }

    MessageData<string> Notes { get; }
}