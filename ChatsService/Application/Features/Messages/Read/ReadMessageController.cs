using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Messages.Read;

[ApiController]
[Route("api/messages")]
public class ReadMessageController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost("{messageId:guid}/read")]
    public async Task<IActionResult> CreateMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var command = new ReadMessageCommand(User.GetId(), messageId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}