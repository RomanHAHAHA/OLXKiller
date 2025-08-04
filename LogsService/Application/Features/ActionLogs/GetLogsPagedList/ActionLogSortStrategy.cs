using System.Linq.Expressions;
using Common.Domain.Interfaces;
using LogsService.Domain.Entiites;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public class ActionLogSortStrategy : ISortStrategy<ActionLog>
{
    public Expression<Func<ActionLog, object>> GetKeySelector(string? orderBy)
    {
        return orderBy switch
        {
            nameof(ActionLog.UserId) => l => l.UserId,
            nameof(ActionLog.ActionType) => l => l.ActionType,
            nameof(ActionLog.CreatedAt) => l => l.CreatedAt,
            _ => l => l.Id
        };
    }
}