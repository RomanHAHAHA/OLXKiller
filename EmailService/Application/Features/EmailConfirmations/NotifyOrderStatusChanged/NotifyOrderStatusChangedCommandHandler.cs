using EmailService.Domain.Interfaces;
using MediatR;

namespace EmailService.Application.Features.EmailConfirmations.NotifyOrderStatusChanged;

public class NotifyOrderStatusChangedCommandHandler(
    IEmailSender emailSender) : IRequestHandler<NotifyOrderStatusChangedCommand>
{
    public async Task Handle(NotifyOrderStatusChangedCommand request, CancellationToken cancellationToken)
    {
        await emailSender.SendMessageAsync(
            request.UserEmail, 
            request.Subject,
            request.Content,
            cancellationToken);
    }
}