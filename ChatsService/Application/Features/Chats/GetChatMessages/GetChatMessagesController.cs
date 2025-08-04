using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Chats.GetChatMessages;

[ApiController]
[Route("api/chats")]
public class GetChatMessagesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{chatId:guid}/messages")]
    [Authorize]
    public async Task<IActionResult> GetChatMessagesAsync(
        Guid chatId,
        [FromQuery] string? cursor, 
        CancellationToken cancellationToken)
    {
        var query = new GetChatMessagesQuery(chatId, cursor);
        var cursorList = await mediator.Send(query, cancellationToken);
        return Ok(new { cursorList });
    }
}