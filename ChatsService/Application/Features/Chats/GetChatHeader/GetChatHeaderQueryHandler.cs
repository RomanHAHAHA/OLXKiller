using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Chats.GetChatHeader;

public class GetChatHeaderQueryHandler(
    ChatsDbContext dbContext) : IRequestHandler<GetChatHeaderQuery, ApiResponse<UserChatHeaderDto>>
{
    public async Task<ApiResponse<UserChatHeaderDto>> Handle(
        GetChatHeaderQuery request, 
        CancellationToken cancellationToken)
    {
        var userChatHeaderDto = await dbContext.Chats
            .AsNoTracking()
            .Where(c => c.Id == request.ChatId)
            .Include(c => c.Users)
            .Select(c => c.Users.FirstOrDefault(u => u.Id != request.CurrentUserId)!)
            .Select(u => new UserChatHeaderDto
            {
                Id = u.Id,    
                NickName = u.NickName,
                AvatarImageName = u.AvatarImageName,
                IsOnline = u.IsOnline,
                LastOnlineAt = u.LastOnlineAt.GetValueOrDefault().ToLocalTime()
            })
            .FirstOrDefaultAsync(cancellationToken);
            
        return userChatHeaderDto is null ? 
            ApiResponse<UserChatHeaderDto>.NotFound("User") :
            ApiResponse<UserChatHeaderDto>.Ok(userChatHeaderDto);
    }
}