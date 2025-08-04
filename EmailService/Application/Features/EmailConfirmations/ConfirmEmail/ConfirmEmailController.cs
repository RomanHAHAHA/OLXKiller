using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Application.Features.EmailConfirmations.ConfirmEmail;

[Route("api/email-confirmations")]
[ApiController]
public class ConfirmEmailController(IMediator mediator) : ControllerBase
{
    [HttpPost("confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailAsync(
        ConfirmEmailCommand command, 
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}