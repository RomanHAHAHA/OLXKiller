using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Messages.Create;

public record CreateMessageCommand(
    Guid CurrentUserId, 
    Guid ChatId,
    string Content) : IRequest<ApiResponse<Guid>>;