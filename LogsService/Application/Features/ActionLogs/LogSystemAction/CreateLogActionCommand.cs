using Common.Domain.Enums;
using MediatR;

namespace LogsService.Application.Features.ActionLogs.LogSystemAction;

public record CreateLogActionCommand(
    Guid UserId,
    ActionType ActionType,
    string Message) : IRequest;