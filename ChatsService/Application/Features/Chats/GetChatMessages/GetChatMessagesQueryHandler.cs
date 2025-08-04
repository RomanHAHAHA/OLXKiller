using ChatsService.Application.Features.Messages.Create;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Chats.GetChatMessages;

public class GetChatMessagesQueryHandler(
    ChatsDbContext dbContext) : IRequestHandler<GetChatMessagesQuery, CursorPagedList<MessageDto>>
{
    private const int Limit = 20;
    
    public async Task<CursorPagedList<MessageDto>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var messagesQuery = dbContext.Messages
            .Where(m => m.ChatId == request.ChatId);

        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            if (!Guid.TryParse(request.Cursor, out var lastId))
            {
                return CursorPagedList<MessageDto>.Empty();
            }
            
            messagesQuery = messagesQuery.Where(m => m.Id < lastId);
        }

        var messages = await messagesQuery
            .OrderByDescending(m => m.Id) 
            .Take(Limit + 1)
            .Select(m => MessageDto.Create(m))
            .ToListAsync(cancellationToken);

        var hasMore = messages.Count > Limit;
        
        if (hasMore)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        return new CursorPagedList<MessageDto>
        {
            Items = messages.OrderBy(m => m.CreatedAt).ToList(),
            Cursor = messages.LastOrDefault()?.Id.ToString(),
            HasMore = hasMore
        };
    }
}