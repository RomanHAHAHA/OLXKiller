using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogsService.Application.Features.ActionLogs.DeleteLog;

[Route("api/action-logs")]
[ApiController]
public class DeleteActionLogController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{actionLogId:guid}")]
    [HasPermission(PermissionEnum.ManageActionLogs)]
    public async Task<IActionResult> DeleteActionLogAsync(
        Guid actionLogId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteActionLogCommand(actionLogId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}