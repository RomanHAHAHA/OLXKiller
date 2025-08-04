using Common.Domain.Abstractions;
using Common.Domain.Enums;

namespace Common.Infrastructure.Messaging.Events.SystemAction;

public class SystemActionEvent : BaseEvent
{
    public required Guid UserId { get; init; }

    public required ActionType ActionType { get; init; }

    public required string Message { get; init; } 
}