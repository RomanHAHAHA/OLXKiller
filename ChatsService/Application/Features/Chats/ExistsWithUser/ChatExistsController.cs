using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ChatsService.Application.Features.Chats.ExistsWithUser;

[ApiController]
[Route("api/chats")]
public class ChatExistsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{otherUserId:guid}/exists")]
    [Authorize]
    public async Task<IActionResult> ChatExistsAsync(
        Guid otherUserId,
        CancellationToken cancellationToken)
    {
        var query = new CheckChatWithUserExistsQuery(User.GetId(), otherUserId);
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}