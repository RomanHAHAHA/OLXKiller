using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Application.Features.EmailConfirmations.SendCode;

[Route("api/email-confirmations")]
[ApiController]
public class SendVerificationCodeController(IMediator mediator) : ControllerBase
{
    [HttpPost("code")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationCodeAsync(
        SendVerificationCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}