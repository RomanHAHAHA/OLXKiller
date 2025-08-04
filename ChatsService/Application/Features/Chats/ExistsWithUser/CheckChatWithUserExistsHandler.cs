using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Chats.ExistsWithUser;

public class CheckChatWithUserExistsHandler(
    ChatsDbContext dbContext) : IRequestHandler<CheckChatWithUserExistsQuery, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(
        CheckChatWithUserExistsQuery request, 
        CancellationToken cancellationToken)
    {
        var chat = await dbContext.Chats
            .AsNoTracking()
            .Include(c => c.Users)
            .Where(c => c.Users.Any(u => u.Id == request.CurrentUserId))
            .Where(c => c.Users.Any(u => u.Id == request.OtherUserId))
            .FirstOrDefaultAsync(cancellationToken);

        return chat is null ? 
            ApiResponse<Guid>.NotFound(nameof(Chat)) : 
            ApiResponse<Guid>.Ok(chat.Id);
    }
}