using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService.Application.Features.Users.GetById;

[Route("/api/users")]
[ApiController]
public class GetUserByIdController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return this.HandleResponse(response);
    }
}