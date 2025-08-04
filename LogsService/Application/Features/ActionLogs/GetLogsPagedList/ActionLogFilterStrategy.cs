using Common.Domain.Extensions;
using Common.Domain.Interfaces;
using LogsService.Domain.Entiites;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public class ActionLogFilterStrategy : IFilterStrategy<ActionLog, ActionLogFilter>
{
    public IQueryable<ActionLog> Filter(IQueryable<ActionLog> query, ActionLogFilter filter)
    {
        return query
            .WhereIf(filter.UserId.HasValue, l => l.UserId == filter.UserId)
            .WhereIf(filter.ActionType.HasValue, l => l.ActionType == filter.ActionType)
            .WhereIf(filter.StartDate.HasValue, l => l.CreatedAt >= filter.StartDate)
            .WhereIf(filter.EndDate.HasValue, l => l.CreatedAt <= filter.EndDate);
    }
}