using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Chats.GetChatHeader;

[ApiController]
[Route("api/chats")]
public class GetChatHeaderController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("{chatId:guid}/header")]
    public async Task<IActionResult> GetChatHeader(Guid chatId)
    {
        var query = new GetChatHeaderQuery(chatId, User.GetId());
        var response = await mediator.Send(query);
        return this.HandleResponse(response);
    }
}