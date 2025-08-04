using MediatR;

namespace EmailService.Application.Features.EmailConfirmations.NotifyOrderStatusChanged;

public record NotifyOrderStatusChangedCommand(
    string UserEmail,
    string Subject,
    string Content) : IRequest;