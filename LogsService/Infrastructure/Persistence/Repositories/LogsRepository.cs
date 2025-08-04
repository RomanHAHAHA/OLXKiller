using Common.Domain.Abstractions;
using Common.Domain.Dtos;
using Common.Domain.Entities;
using Common.Domain.Extensions;
using Common.Domain.Models.Results;
using LogsService.Application.Features.ActionLogs.GetLogsPagedList;
using LogsService.Domain.Entiites;
using LogsService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogsService.Infrastructure.Persistence.Repositories;

public class LogsRepository(ActionLogsDbContext dbContext) :
    Repository<ActionLogsDbContext, ActionLog, Guid>(dbContext),
    ILogsRepository
{
    public async Task<PagedList<ActionLog>> GetActionLogsAsync(
        ActionLogFilter actionLogFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken)
    {
        return await AppDbContext.ActionLogs
            .AsNoTracking()
            .Filter(actionLogFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams, cancellationToken);
    }
}