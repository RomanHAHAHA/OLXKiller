using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Chats.GetUserChats;

public class GetUserChatsQueryHandler(
    ChatsDbContext dbContext) : IRequestHandler<GetUserChatsQuery, List<ChatViewDto>>
{
    public async Task<List<ChatViewDto>> Handle(
        GetUserChatsQuery request, 
        CancellationToken cancellationToken)
    {
        var chats = await dbContext.Chats
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Users)
            .Include(c => c.Messages)
            .Where(c => c.Users.Any(u => u.Id == request.CurrentUserId) && c.Messages.Any())
            .Select(c => new ChatViewDto
            {
                Id = c.Id,
                OtherUser = c.Users
                    .Where(u => u.Id != request.CurrentUserId)
                    .Select(u => new UserSnapshot
                    {
                        Id = u.Id,
                        NickName = u.NickName,
                        AvatarImageName = u.AvatarImageName,
                        IsOnline = u.IsOnline,
                        LastOnlineAt = u.LastOnlineAt.GetValueOrDefault().ToLocalTime(),
                    })
                    .FirstOrDefault()!,
                
                UnreadMessages = c.Messages.Count(m => !m.IsRead && m.SenderId != request.CurrentUserId),
                
                IsMuted = dbContext.UserMutes.Any(um =>
                    um.MutingUserId == request.CurrentUserId &&
                    um.MutedUserId == c.Users.FirstOrDefault(u => u.Id != request.CurrentUserId)!.Id),
                
                LastMessageSentAt = c.Messages.Any() 
                    ? c.Messages.Max(m => m.CreatedAt).ToLocalTime() 
                    : DateTime.MinValue
            })
            .ToListAsync(cancellationToken);
        
        return chats.OrderByDescending(c => c.LastMessageSentAt).ToList();
    }
}