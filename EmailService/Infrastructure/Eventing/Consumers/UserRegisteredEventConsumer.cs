using Common.Infrastructure.Messaging.Events.User;
using EmailService.Application.Features.EmailConfirmations.SendCode;
using MassTransit;
using MediatR;

namespace EmailService.Infrastructure.Eventing.Consumers;

public class UserRegisteredEventConsumer(IMediator mediator) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;
        var command = new SendVerificationCodeCommand(@event.Email);
        await mediator.Send(command, context.CancellationToken);
    }
}