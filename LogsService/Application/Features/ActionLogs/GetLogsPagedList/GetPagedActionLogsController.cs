using Common.API.Authentication;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

[Route("api/action-logs")]
[ApiController]
public class GetPagedActionLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionEnum.ManageActionLogs)]
    public async Task<PagedList<PagedActionLogDto>> GetActionLogsAsync(
        [FromQuery] ActionLogFilter filter,
        [FromQuery] SortParams sortParams,
        [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var query = new GetActionLogsQuery(filter, sortParams, pageParams);
        return await mediator.Send(query, cancellationToken);
    }
}