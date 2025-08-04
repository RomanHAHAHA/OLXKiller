using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ChatsService.Application.Features.Messages.Create;

[ApiController]
[Route("api/chats")]
public class CreateMessageController(IMediator mediator) : ControllerBase
{
    [HttpPost("{chatId:guid}/messages")]
    [Authorize]
    public async Task<IActionResult> CreateMessageAsync(
        Guid chatId,
        [FromQuery] string content,
        CancellationToken cancellationToken)
    {
        var command = new CreateMessageCommand(User.GetId(), chatId, content);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}