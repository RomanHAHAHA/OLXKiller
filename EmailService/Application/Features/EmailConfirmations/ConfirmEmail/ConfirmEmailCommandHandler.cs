using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Email;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Features.EmailConfirmations.ConfirmEmail;

public class ConfirmEmailCommandHandler(
    ICacheService<string> cacheService,
    IPasswordHasher passwordHasher,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<ConfirmEmailCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var hashedCode = await cacheService.GetAsync(request.Email, cancellationToken);

        if (hashedCode is null)
        {
            return ApiResponse.Conflict("Confirmation time was expired");
        }
        
        if (!passwordHasher.Verify(request.Code, hashedCode))
        {
            return ApiResponse.BadRequest("Invalid code");
        }
        
        await cacheService.RemoveAsync(request.Email, cancellationToken);
        await OnEmailConfirmed(request.Email, cancellationToken);
        
        return ApiResponse.Ok();
    }

    private async Task OnEmailConfirmed(string email, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new EmailConfirmedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                Email = email
            }, 
            cancellationToken);
    }
}