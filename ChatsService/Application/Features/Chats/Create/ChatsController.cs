using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Chats.Create;

[ApiController]
[Route("api/chats")]
public class ChatsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{otherUserId:guid}")]
    [Authorize]
    public async Task<IActionResult> CreateChatAsync(
        Guid otherUserId,
        CancellationToken cancellationToken)
    {
        var command = new CreateChatCommand(User.GetId(), otherUserId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}