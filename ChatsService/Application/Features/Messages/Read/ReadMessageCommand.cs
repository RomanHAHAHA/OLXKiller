using ChatsService.Domain.Entities;
using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Messages.Read;

public record ReadMessageCommand(
    Guid CurrentUserId,
    Guid MessageId) : IRequest<ApiResponse<Guid>>;