using Common.Domain.Models.Results;
using LogsService.Domain.Interfaces;
using MediatR;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public class GetActionLogsQueryHandler(
    ILogsRepository logsRepository) : IRequestHandler<GetActionLogsQuery, PagedList<PagedActionLogDto>>
{
    public async Task<PagedList<PagedActionLogDto>> Handle(
        GetActionLogsQuery request, 
        CancellationToken cancellationToken)
    {
        var pagedLogs = await logsRepository.GetActionLogsAsync(
            request.ActionLogFilter, 
            request.SortParams,
            request.PageParams,
            cancellationToken);

        var dtos = pagedLogs.Items.Select(l => new PagedActionLogDto
        {
            Id = l.Id,
            UserId = l.UserId,
            ActionType = l.ActionType.ToString(),
            Description = l.Description,
            CreatedAt = $"{l.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
        }).ToList();
        
        return new PagedList<PagedActionLogDto>(
            dtos,
            pagedLogs.Page,
            pagedLogs.PageSize,
            pagedLogs.TotalCount);
    }
}