namespace Common.Domain.Abstractions;

public abstract class BaseEvent
{
    public required Guid CorrelationId { get; init; }
    
    public required string SenderServiceName { get; init; }
}