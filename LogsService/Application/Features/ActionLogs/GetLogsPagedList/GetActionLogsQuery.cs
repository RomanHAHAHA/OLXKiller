using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public record GetActionLogsQuery(
    ActionLogFilter ActionLogFilter,
    SortParams SortParams,
    PageParams PageParams) : IRequest<PagedList<PagedActionLogDto>>;