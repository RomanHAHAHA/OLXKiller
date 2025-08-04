using Common.Domain.Dtos;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using LogsService.Application.Features.ActionLogs.GetLogsPagedList;
using LogsService.Domain.Entiites;

namespace LogsService.Domain.Interfaces;

public interface ILogsRepository : IRepository<ActionLog, Guid>
{
    Task<PagedList<ActionLog>> GetActionLogsAsync(
        ActionLogFilter actionLogFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken);
}