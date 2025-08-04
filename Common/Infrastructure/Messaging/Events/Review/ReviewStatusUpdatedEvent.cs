using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Review;

public class ReviewStatusUpdatedEvent : BaseEvent
{
    public required Guid ProductId { get; init; } 
}