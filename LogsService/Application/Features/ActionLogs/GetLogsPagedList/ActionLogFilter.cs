using Common.Domain.Enums;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public record ActionLogFilter(
    Guid? UserId,
    ActionType? ActionType,
    DateTime? StartDate,
    DateTime? EndDate);