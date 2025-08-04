using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ChatsService.Application.Features.Chats.GetUserChats;

[ApiController]
[Route("api/chats")]
public class GetUserChatsController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetUserChats(CancellationToken cancellationToken)
    {
        var query = new GetUserChatsQuery(User.GetId());
        var chats  = await mediator.Send(query, cancellationToken);
        return Ok(new { data = chats});
    } 
}